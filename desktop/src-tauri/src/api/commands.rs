use crate::api::storage;
use crate::appdata::AppData;
use crate::appdata::AppState;
use crate::appdata::ExperimentState;
use crate::consts::Folder;
use crate::structs::CreateExperiment;
use crate::structs::Experiment;
use crate::structs::ExperimentAnswer;
use crate::structs::ExperimentResult;
use crate::structs::Parameter;
use crate::structs::ParameterKey;
use crate::structs::ParameterValues;
use crate::structs::Preset;

use itertools::Itertools;
use local_ip_address::local_ip;
use serde::Deserialize;
use serde::Serialize;
use slug::slugify;
use specta::Type;
use std::collections::HashMap;
use tauri::Manager;

#[derive(Deserialize, Serialize, Type)]
pub struct WithKey<T> {
    pub key: String,
    pub value: T,
}

#[specta::specta]
#[tauri::command]
pub fn current_state(app: tauri::AppHandle) -> AppState {
    let app_data = app.state::<AppData>();

    app_data.state.get_cloned()
}

#[specta::specta]
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
#[specta::specta]
pub fn get_parameters() -> Vec<Parameter> {
    Parameter::all()
}

/// Set a parameter in live view
#[tauri::command]
#[specta::specta]
pub fn set_live_parameter(
    app: tauri::AppHandle,
    parameter: ParameterKey,
    value: f32,
) -> Result<(), String> {
    let app_data = app.state::<AppData>();
    let mut app_state = app_data.state.lock_mut();

    let live_state = app_state
        .try_as_live_view_mut()
        .ok_or("Must be in live mode".to_owned())?;

    live_state.set(parameter, value);

    Ok(())
}

#[tauri::command]
#[specta::specta]
pub fn get_live_parameter(app: tauri::AppHandle, parameter: ParameterKey) -> Result<f32, String> {
    let app_data = app.state::<AppData>();
    let app_state = app_data.state.lock_ref();

    let live_state = app_state
        .try_as_live_view_ref()
        .ok_or("Must be in live mode")?;

    let param = live_state.get(parameter);
    Ok(param)
}

#[tauri::command]
#[specta::specta]
pub fn create_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
    // Parse PARAMS to JSON
    let app_data = app.state::<AppData>();
    let app_state = app_data.state.lock_ref();

    let parameters = app_state
        .try_as_live_view_ref()
        .ok_or("Must be in live mode".to_owned())?;

    storage::create_and_write_to_json_file(parameters, Folder::Presets, preset_name)
}

#[tauri::command]
#[specta::specta]
pub async fn get_presets() -> Result<Vec<WithKey<Preset>>, String> {
    storage::get_files(Folder::Presets).await
}

#[tauri::command]
#[specta::specta]
pub fn delete_preset(key: String) -> Result<(), String> {
    storage::delete_file(key, Folder::Presets)
}

/// Get all experiments
#[tauri::command]
#[specta::specta]
pub async fn get_experiments() -> Result<Vec<WithKey<Experiment>>, String> {
    storage::get_files(Folder::Experiments).await
}

#[tauri::command]
#[specta::specta]
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

    //Generate file name <SLUG OF EXPERIMENT NAME>
    let file_name = slugify(&experiment.name);

    //Create and write to JSON file
    storage::create_and_write_to_json_file(&experiment, Folder::Experiments, file_name)?;
    //TODO: Kan eventuelt returnere det nye eksperimentet sånn det kan vises på frontend som en slags bekreftelse
    Ok(String::from("Experiment created successfully"))
}

#[tauri::command]
#[specta::specta]
pub fn start_experiment(
    app: tauri::AppHandle,
    slugged_experiment_name: String,
    obeserver_id: u32,
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

    // Update the AppState in AppData to be in "ExperimentMode"
    let app_data = app.state::<AppData>();
    let app_state = &app_data.state;

    app_state.set(AppState::Experiment(ExperimentState::new(
        experiment,
        experiment_result,
    )));

    //Return success signal to frontend
    Ok(())
}

/// Exit the current experiment early
#[tauri::command]
#[specta::specta]
pub fn exit_experiment(app: tauri::AppHandle) {
    let app_data = app.state::<AppData>();

    app_data.state.set(AppState::LiveView(ParameterValues {
        // TODO Fikse med endringer fra oliver
        ..Default::default()
    }));
}

/// Answer the current experiment prompt
#[tauri::command]
#[specta::specta]
pub fn answer_experiment(app: tauri::AppHandle, answer: ExperimentAnswer) -> Result<(), String> {
    let app_data = app.state::<AppData>();
    let mut app_state = app_data.state.lock_mut();

    app_state.answer_experiment(answer)
}

/// Swap the current preset in the experiment
#[tauri::command]
#[specta::specta]
pub fn swap_preset(app: tauri::AppHandle) -> Result<(), String> {
    let app_data = app.state::<AppData>();
    let mut app_state = app_data.state.lock_mut();

    app_state.swap_current_preset()
}
