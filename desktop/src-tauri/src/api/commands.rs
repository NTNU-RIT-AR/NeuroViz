pub mod commands {

    use crate::structs::Experiment;
    use crate::structs::CreateExperiment;
    use crate::structs::Preset;
    use crate::structs::RenderParams;
    use crate::consts::Folder;
    // use crate::consts::FOLDER_PRESETS;
    // use crate::consts::FOLDER_EXPERIMENTS;
    use crate::api::storage::storage;

    use local_ip_address::local_ip;
    use tauri::Manager;
    use std::collections::HashMap;
    use slug::slugify;

    #[tauri::command]
    pub fn get_ip_address() -> String {
        match local_ip() {
            Ok(ip) => {
                println!("Local IP address: {}", ip);
                return format!("{}", ip);
            }
            Err(e) => {
                eprintln!("Error retrieving local IP: {}", e);
                return String::from("");
            }
        }
    }

    #[tauri::command]
    pub fn set_param(app: tauri::AppHandle, param_name: &str, value: f64) {
        let state = app.state::<RenderParams>();
        let mut params = state.lock().unwrap();
        let value = value as f32;
        println!("Updated slider values {:?}", params);
        match param_name {
            "Hue" => params.hue = value,
            "Smoothness" => params.smoothness = value,
            "Metallic" => params.metallic = value,
            "Emission" => params.emission = value,
            _ => {}
        }
    }
    #[tauri::command]
    pub fn get_param(app: tauri::AppHandle, param_name: &str) -> f64 {
        let state = app.state::<RenderParams>();
        let params = state.lock().unwrap();
        println!("Returned param:  {:?}", params);
        return match param_name {
            "Hue" => params.hue,
            "Smoothness" => params.smoothness,
            "Metallic" => params.metallic,
            "Emission" => params.emission,
            _ => panic!("param mismatch"),
        } as f64;
    }

    #[tauri::command]
    pub fn list_presets() -> Result<Vec<String>, String> {
        return storage::list_files(Folder::Presets);
    }

    
    #[tauri::command]
    pub fn list_experiments() -> Result<Vec<String>, String> {
        return storage::list_files(Folder::Experiments);
    }

    //fn delete_file(file_name: String) -> Result<(), String> {
    //    let folder_path = fs::read_to_string(file_name).map_err(|e| e.to_string())?;
    //    let file_path = BaseDirectory::AppData + "/" + &file_name;
    //
    //    println!("Document path: {}", folder_path);
    //
    //    //fs::remove_file(file_path).map_err(|e| e.to_string())?;
    //    Ok(())
    //}

    #[tauri::command]
    pub fn create_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
        // Parse PARAMS to JSON
        let state = app.state::<RenderParams>();
        let params = state.lock().unwrap().clone();
        let json_string = serde_json::to_string(&params).unwrap();

        storage::create_and_write_to_json_file(json_string, Folder::Presets, preset_name)
    }

    #[tauri::command]
    pub fn retrieve_preset(slugged_preset_name: String) -> Result<Preset, String> {
        storage::parse_from_json_file::<Preset>(slugged_preset_name, Folder::Presets)
    }

    #[tauri::command]
    pub fn create_experiment(experiment_init_data: CreateExperiment) -> Result<String, String> {

        //Derive from CreateExperiment and Preset to Experiment
        let mut experiment_presets : HashMap<String, Preset> = HashMap::with_capacity(experiment_init_data.presets.len());

        for preset_name in experiment_init_data.presets {
            experiment_presets.insert(slugify(preset_name.clone()), storage::parse_from_json_file::<Preset>(slugify(preset_name), Folder::Presets)?);
        }

        let experiment : Experiment = Experiment { 
            experiment_type: experiment_init_data.experiment_type,
            name: experiment_init_data.name,
            presets: experiment_presets
        };

        //Parse Experiment to JSON
        let experiment_as_json = match serde_json::to_string(&experiment) {
            Ok(content) => content,
            Err(e) => return Err(format!("Could not parse experiment into JSON before storing it {}", e.to_string()))
        };

        //Generate file name <SLUG OF EXPERIMENT NAME>
        let file_name = slugify(experiment.name);

        //Create and write to JSON file 
        storage::create_and_write_to_json_file(experiment_as_json, Folder::Experiments, file_name)?;
        //TODO: Kan eventuelt returnere det nye eksperimentet sånn det kan vises på frontend som en slags bekreftelse
        Ok(String::from("Experiment created successfully"))
    }

    #[tauri::command]
    pub fn retrieve_experiment(slugged_experiment_name: String) -> Result<Experiment, String> {
        storage::parse_from_json_file::<Experiment>(slugged_experiment_name, Folder::Experiments)
    }

}
