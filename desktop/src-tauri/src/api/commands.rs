use crate::api::storage;
use crate::appdata::AppData;
use crate::appdata::AppState;
use crate::consts::Folder;
use crate::structs::CreateExperiment;
use crate::structs::Experiment;
use crate::structs::ExperimentPrompt;
use crate::structs::ExperimentResult;
use crate::structs::ExperimentState;
use crate::structs::ExperimentType;
use crate::structs::Preset;
use crate::structs::UnityExperimentType;

use local_ip_address::local_ip;
use slug::slugify;
use std::collections::HashMap;
use tauri::Manager;

use super::http_server::UnityState;

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
    let appdata = app.state::<AppData>();
    appdata.set_param(param_name, value);
}

#[tauri::command]
pub fn get_param(app: tauri::AppHandle, param_name: &str) -> f64 {
    let appdata = app.state::<AppData>();
    appdata.get_param(param_name)
}

#[tauri::command]
pub fn list_files(folder: Folder) -> Result<Vec<String>, String> {
    match storage::list_files(folder) {
        Ok(files) => Ok(files),
        Err(e) => Err(format!("could not generate file list: {}", e)),
    }
}

#[tauri::command]
pub fn save_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
    // Parse PARAMS to JSON
    let appdata = app.state::<AppData>();
    let params = appdata.params.lock().unwrap().clone();
    let json_string = serde_json::to_string(&params).unwrap();

    storage::create_and_write_to_json_file(json_string, Folder::Presets, preset_name)
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
    let appdata = app.state::<AppData>();
    let params = appdata.params.lock().unwrap().clone();
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
    let mut experiment_presets: HashMap<String, Preset> =
        HashMap::with_capacity(experiment_init_data.presets.len());

    for preset_name in experiment_init_data.presets {
        experiment_presets.insert(
            slugify(preset_name.clone()),
            storage::parse_from_json_file::<Preset>(slugify(preset_name), Folder::Presets)?,
        );
    }

    let experiment: Experiment = Experiment {
        experiment_type: experiment_init_data.experiment_type,
        name: experiment_init_data.name,
        presets: experiment_presets,
    };

    //Parse Experiment to JSON
    let experiment_as_json = match serde_json::to_string(&experiment) {
        Ok(content) => content,
        Err(e) => {
            return Err(format!(
                "Could not parse experiment into JSON before storing it {}",
                e.to_string()
            ))
        }
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

#[tauri::command]
pub fn start_experiment(
    app: tauri::AppHandle,
    slugged_experiment_name: String,
    obeserver_id: u64,
    note: String,
) -> Result<(), String> {
    //Instansiate ExperimentResult and Experiment for the selected experiment (so that we can eventually store the data to file)
    let experiment: Experiment = match storage::parse_from_json_file::<Experiment>(
        slugged_experiment_name,
        Folder::Experiments,
    ) {
        Ok(e) => e,
        Err(e) => return Err(e),
    };
    let experiment_result: ExperimentResult =
        ExperimentResult::new(&experiment, obeserver_id, note);

    let experiment_type: UnityExperimentType = match experiment.experiment_type {
        ExperimentType::Choice { .. } => UnityExperimentType::Choice,
        ExperimentType::Rating { .. } => UnityExperimentType::Rating,
    };

    //Update the AppState in AppData to be in "ExperimentMode"
    let appdata = app.state::<AppData>();
    appdata.set_state(AppState::ExperimentMode {
        experiment_result,
        experiment,
        experiment_state: ExperimentState::new(),
    });

    //Notify the unity app by signaling the HTTP server (via watch channel) that we have started an experiment. Time to experiment!
    let preset = appdata.get_current_preset()?;

    appdata
        .watch_sender
        .send(UnityState::Experiment {
            prompt: ExperimentPrompt {
                experiment_type,
                preset,
            },
        })
        .unwrap();

    //Return success signal to frontend
    Ok(())
}
