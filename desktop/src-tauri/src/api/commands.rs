pub mod commands {

    use crate::structs::RenderParams;
    use crate::api::storage::storage;

    use super::super::super::consts::FOLDER_PRESETS;
    use super::super::super::consts::FOLDER_EXPERIMENTS;
    use super::super::super::consts::FOLDER_RESULTS;
    use local_ip_address::local_ip;
    use tauri::Manager;

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
        return storage::list_files(FOLDER_PRESETS);
    }

    
    #[tauri::command]
    pub fn list_experiments() -> Result<Vec<String>, String> {
        return storage::list_files(FOLDER_EXPERIMENTS);
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
    pub fn save_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
        // Parse PARAMS to JSON
        let state = app.state::<RenderParams>();
        let params = state.lock().unwrap().clone();
        let json_string = serde_json::to_string(&params).unwrap();

        storage::create_and_write_to_json_file(json_string, FOLDER_PRESETS.to_string(), preset_name)
    }

    #[tauri::command]
    pub fn retrieve_preset(preset_name: String) -> Result<String, String> {
        storage::read_from_json_file(FOLDER_PRESETS, format!("{}.json", preset_name))
    }

}
