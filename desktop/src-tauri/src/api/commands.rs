use crate::api::storage;
use crate::appdata::AppData;
use crate::appdata::AppState;
use crate::appdata::ExperimentState;
use crate::consts::Folder;
use crate::extensions::WatchSenderExt;
use crate::structs::CreateExperiment;
use crate::structs::Experiment;
use crate::structs::ExperimentAnswer;
use crate::structs::ExperimentResult;
use crate::structs::Parameter;
use crate::structs::ParameterValues;
use crate::structs::Preset;

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
    let state = app_data.state.borrow().clone();

    state
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

#[tauri::command]
#[specta::specta]
pub async fn get_presets() -> Result<Vec<WithKey<Preset>>, String> {
    storage::read_files(Folder::Presets).await
}

#[tauri::command]
#[specta::specta]
pub async fn create_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
    // Parse PARAMS to JSON
    let app_data = app.state::<AppData>();

    let parameters = {
        let app_state = app_data.state.borrow();

        app_state
            .try_as_live_view_ref()
            .ok_or("Must be in live mode".to_owned())?
            .clone()
    };

    storage::create_file(preset_name, parameters, Folder::Presets).await
}

#[tauri::command]
#[specta::specta]
pub async fn delete_preset(key: String) -> Result<(), String> {
    storage::delete_file(key, Folder::Presets).await
}

/// Get all experiments
#[tauri::command]
#[specta::specta]
pub async fn get_experiments() -> Result<Vec<WithKey<Experiment>>, String> {
    storage::read_files(Folder::Experiments).await
}

#[tauri::command]
#[specta::specta]
pub async fn create_experiment(experiment_init_data: CreateExperiment) -> Result<String, String> {
    //Derive from CreateExperiment and Preset to Experiment
    let mut experiment_presets: HashMap<String, Preset> =
        HashMap::with_capacity(experiment_init_data.presets.len());

    for preset_name in experiment_init_data.presets {
        experiment_presets.insert(
            slugify(preset_name.clone()),
            storage::read_file::<Preset>(slugify(preset_name), Folder::Presets).await?,
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
    storage::create_file(file_name, &experiment, Folder::Experiments).await?;
    //TODO: Kan eventuelt returnere det nye eksperimentet sånn det kan vises på frontend som en slags bekreftelse
    Ok(String::from("Experiment created successfully"))
}

/// Delete an experiment
#[tauri::command]
#[specta::specta]
pub async fn delete_experiment(key: String) -> Result<(), String> {
    storage::delete_file(key, Folder::Experiments).await
}

/// Get all parameters in live view
#[tauri::command]
#[specta::specta]
pub fn get_live_parameters(app: tauri::AppHandle) -> Result<ParameterValues, String> {
    let app_data = app.state::<AppData>();
    let app_state = app_data.state.borrow();

    let live_state = app_state
        .try_as_live_view_ref()
        .ok_or("Must be in live mode".to_owned())?;

    Ok(live_state.clone())
}

/// Set all parameters in live view
#[tauri::command]
#[specta::specta]
pub fn set_live_parameters(
    app: tauri::AppHandle,
    parameters: ParameterValues,
) -> Result<(), String> {
    let app_data = app.state::<AppData>();

    app_data.state.send_modify_with(|app_state| {
        let live_state = app_state
            .try_as_live_view_mut()
            .ok_or("Must be in live mode".to_owned())?;

        *live_state = parameters;

        Ok(())
    })
}

#[tauri::command]
#[specta::specta]
pub async fn start_experiment(
    app: tauri::AppHandle,
    slugged_experiment_name: String,
    obeserver_id: u32,
    note: String,
) -> Result<(), String> {
    //Instansiate ExperimentResult and Experiment for the selected experiment (so that we can eventually store the data to file)
    let experiment: Experiment = match storage::read_file::<Experiment>(
        slugged_experiment_name,
        Folder::Experiments,
    )
    .await
    {
        Ok(e) => e,
        Err(e) => return Err(e),
    };
    let experiment_result: ExperimentResult =
        ExperimentResult::new(&experiment, obeserver_id, note);

    // Update the AppState in AppData to be in "ExperimentMode"
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send(AppState::Experiment(ExperimentState::new(
            experiment,
            experiment_result,
        )))
        .unwrap();

    //Return success signal to frontend
    Ok(())
}

/// Exit the current experiment early
#[tauri::command]
#[specta::specta]
pub fn exit_experiment(app: tauri::AppHandle) {
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send(AppState::LiveView(ParameterValues {
            // TODO Fikse med endringer fra oliver
            ..Default::default()
        }))
        .unwrap();
}

/// Answer the current experiment prompt
#[tauri::command]
#[specta::specta]
pub fn answer_experiment(app: tauri::AppHandle, answer: ExperimentAnswer) -> Result<(), String> {
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send_modify_with(|state| state.answer_experiment(answer))
}

/// Swap the current preset in the experiment
#[tauri::command]
#[specta::specta]
pub fn swap_preset(app: tauri::AppHandle) -> Result<(), String> {
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send_modify_with(|state| state.swap_current_preset())
}
