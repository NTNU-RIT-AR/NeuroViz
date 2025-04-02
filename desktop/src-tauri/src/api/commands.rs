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
use slug::slugify;
use std::collections::HashMap;
use tauri::Manager;

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
pub fn get_parameters() -> Vec<Parameter> {
    Parameter::all()
}

/// Get a parameter in live view
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

/// List all files in a given folder
#[tauri::command]
#[specta::specta]
pub fn list_files(folder: Folder) -> Result<Vec<String>, String> {
    match storage::list_files(folder) {
        Ok(files) => Ok(files),
        Err(e) => Err(format!("could not generate file list: {}", e)),
    }
}

/// Save current live parameters to a preset
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

/// List all presets
#[tauri::command]
#[specta::specta]
pub fn list_presets() -> Result<Vec<String>, String> {
    return storage::list_files(Folder::Presets);
}

/// List all experiments
#[tauri::command]
#[specta::specta]
pub fn list_experiments() -> Result<Vec<String>, String> {
    return storage::list_files(Folder::Experiments);
}

/// Retrieve a preset by file name
#[tauri::command]
#[specta::specta]
pub fn get_preset(slugged_preset_name: String) -> Result<Preset, String> {
    storage::parse_from_json_file::<Preset>(slugged_preset_name, Folder::Presets)
}

#[tauri::command]
#[specta::specta]
pub fn get_all_presets() -> Result<Vec<Preset>, String> {
    list_presets()?
        .into_iter()
        .map(|preset| get_preset(preset))
        .try_collect()
}

#[tauri::command]
#[specta::specta]
pub fn delete_preset(slugged_name: String) -> Result<(), String> {
    storage::delete_json_file(slugged_name, Folder::Presets)
}

/// Create a new experiment
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

/// Retrieve a preset
#[tauri::command]
#[specta::specta]
pub fn get_experiment(slugged_name: String) -> Result<Experiment, String> {
    storage::parse_from_json_file::<Experiment>(slugged_name, Folder::Experiments)
}

/// Start an experiment
#[tauri::command]
#[specta::specta]
pub fn get_all_experiments() -> Result<Vec<Experiment>, String> {
    let mut result: Vec<Experiment> = Vec::new();

    let experiments = list_experiments()?;
    for experiment in experiments {
        result.push(get_experiment(experiment)?);
    }
    Ok(result)
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
