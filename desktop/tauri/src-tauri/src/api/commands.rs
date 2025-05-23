use anyhow::Context;
use chrono::Local;
use local_ip_address::local_ip;
use neuroviz::{
    extensions::WatchSenderExt,
    http_server::ExperimentAnswer,
    parameters::{Parameter, ParameterValues},
};
use rand::seq::SliceRandom;
use serde::{Deserialize, Serialize};
use serde_json::Value;
use slug::slugify;
use specta::Type;
use std::{collections::HashMap, path::PathBuf, time::Duration};
use tauri::Manager;
use tauri_plugin_opener::OpenerExt;
use tauri_specta::Event;
use tokio::{fs, time::sleep};

use crate::{
    data::{
        experiment::{
            ChoiceExperiment, CreateExperiment, CreateExperimentType, Experiment, RatingExperiment,
        },
        experiment_result::{ChoiceExperimentResult, ExperimentResult, RatingExperimentResult},
        folder::TopLevelFolder,
        preset::Preset,
    },
    state::{experiment_state::ExperimentState, AppData, AppState},
    storage::{self, Folder},
};

use super::{command_error::AppError, events::ResultSavedEvent};

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
pub fn is_connected(app: tauri::AppHandle) -> bool {
    let app_data = app.state::<AppData>();

    let connected_clients = *app_data.connected_clients.borrow();

    connected_clients > 0
}

#[specta::specta]
#[tauri::command]
pub fn show_folder(app: tauri::AppHandle, folder: TopLevelFolder) -> Result<(), AppError> {
    let path = storage::data_folder()?.join(folder.path());

    app.opener()
        .open_path(path.to_string_lossy(), None::<&str>)
        .context("Open folder")?;

    Ok(())
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

#[specta::specta]
#[tauri::command]
pub fn get_secret(app: tauri::AppHandle) -> String {
    let app_data = app.state::<AppData>();
    let secret = (*app_data.secret).clone();

    secret
}

#[tauri::command]
#[specta::specta]
pub fn get_parameters() -> Vec<Parameter> {
    Parameter::all().collect()
}

#[tauri::command]
#[specta::specta]
pub fn get_default_parameters() -> ParameterValues {
    ParameterValues::default()
}

#[tauri::command]
#[specta::specta]
pub async fn get_presets() -> Result<Vec<WithKey<Preset>>, AppError> {
    let presets = storage::read_files(Folder::Presets).await?;

    Ok(presets)
}

#[tauri::command]
#[specta::specta]
pub async fn create_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), AppError> {
    // Parse PARAMS to JSON
    let app_data = app.state::<AppData>();
    let preset_key = slugify(&preset_name);

    let parameters = {
        let app_state = app_data.state.borrow();

        app_state
            .try_as_live_view_ref()
            .context("Must be in live mode")?
            .clone()
    };

    let preset = Preset {
        name: preset_name,
        parameters,
    };

    storage::create_file(&preset_key, preset, Folder::Presets).await?;

    Ok(())
}

#[tauri::command]
#[specta::specta]
pub async fn delete_preset(key: String) -> Result<(), AppError> {
    storage::delete_file(&key, Folder::Presets).await?;

    Ok(())
}

/// Get all experiments
#[tauri::command]
#[specta::specta]
pub async fn get_experiments() -> Result<Vec<WithKey<Experiment>>, AppError> {
    let experiments = storage::read_files(Folder::Experiments).await?;

    Ok(experiments)
}

#[tauri::command]
#[specta::specta]
pub async fn create_experiment(
    experiment_init_data: CreateExperiment,
) -> Result<PathBuf, AppError> {
    //Generate file name <SLUG OF EXPERIMENT NAME>
    let experiment_key = slugify(&experiment_init_data.name);

    //Derive from CreateExperiment and Preset to Experiment
    let mut experiment_presets: HashMap<String, Preset> =
        HashMap::with_capacity(experiment_init_data.presets.len());

    for preset_name in experiment_init_data.presets {
        experiment_presets.insert(
            slugify(preset_name.clone()),
            storage::read_file::<Preset>(&slugify(preset_name), Folder::Presets).await?,
        );
    }

    let experiment = match experiment_init_data.experiment_type {
        CreateExperimentType::Rating { order } => Experiment::Rating(RatingExperiment::new(
            experiment_init_data.name,
            experiment_presets,
            order,
        )),
        CreateExperimentType::Choice { choices } => Experiment::Choice(ChoiceExperiment::new(
            experiment_init_data.name,
            experiment_presets,
            choices,
        )),
    };

    let path = storage::create_file(&experiment_key, &experiment, Folder::Experiments).await?;
    Ok(path)
}

/// Delete an experiment
#[tauri::command]
#[specta::specta]
pub async fn delete_experiment(key: String) -> Result<(), AppError> {
    storage::delete_file(&key, Folder::Experiments).await?;

    Ok(())
}

/// Enter idle
#[tauri::command]
#[specta::specta]
pub fn set_idle_mode(app: tauri::AppHandle) -> Result<(), AppError> {
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send(AppState::Idle)
        .context("Send new app state")?;

    Ok(())
}

/// Enter live mode with parameters
#[tauri::command]
#[specta::specta]
pub fn set_live_mode(app: tauri::AppHandle, parameters: ParameterValues) -> Result<(), AppError> {
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send(AppState::LiveView(parameters))
        .context("Send new app state")?;

    Ok(())
}

#[tauri::command]
#[specta::specta]
pub async fn start_experiment(
    app: tauri::AppHandle,
    experiment_key: String,
    result_name: String,
    obeserver_id: u32,
    note: String,
    randomize: bool,
) -> Result<(), AppError> {
    let result_key = slugify(&result_name);

    let experiment = storage::read_file::<Experiment>(&experiment_key, Folder::Experiments).await?;

    let time = Local::now();
    let mut rng = rand::rng();

    let experiment_state = match experiment {
        Experiment::Rating(mut rating_experiment) => {
            if randomize {
                rating_experiment.order.shuffle(&mut rng);
            }

            let experiment_result = RatingExperimentResult::new(
                result_name,
                time,
                obeserver_id,
                note,
                &rating_experiment,
            );

            ExperimentState::new_rating(
                experiment_key,
                result_key,
                rating_experiment,
                experiment_result,
            )
        }

        Experiment::Choice(mut choice_experiment) => {
            if randomize {
                choice_experiment.choices.shuffle(&mut rng);
            }

            let experiment_result = ChoiceExperimentResult::new(
                result_name,
                time,
                obeserver_id,
                note,
                &choice_experiment,
            );

            ExperimentState::new_choice(
                experiment_key,
                result_key,
                choice_experiment,
                experiment_result,
            )
        }
    };

    // Update the AppState in AppData to be in "ExperimentMode"
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send(AppState::Experiment(experiment_state))
        .unwrap();

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
pub async fn answer_experiment(
    app: tauri::AppHandle,
    answer: ExperimentAnswer,
) -> Result<(), AppError> {
    let app_data = app.state::<AppData>();

    let experiment_is_done = app_data.state.send_modify_with(|state| {
        let is_done = state.answer_experiment(answer);

        if let Some(experiment_state) = state.try_as_experiment_mut() {
            experiment_state.set_is_idle(true);
        }

        is_done
    })?;

    // Sleep for a second while idle
    if !experiment_is_done {
        sleep(Duration::from_secs(1)).await;

        app_data.state.send_modify(|state| {
            if let Some(experiment_state) = state.try_as_experiment_mut() {
                experiment_state.set_is_idle(false);
            }
        });
    }

    // If the experiment is done, finish it and emit the result saved event
    if experiment_is_done {
        let experiment_state = app_data
            .state
            .send_replace(AppState::Idle)
            .try_as_experiment()
            .context("Must be in experiment")?;

        let result_file_path = experiment_state.finish_experiment().await?;

        ResultSavedEvent { result_file_path }
            .emit(&app)
            .context("Could not emit event")?;
    }

    Ok(())
}

/// Swap the current preset in the experiment
#[tauri::command]
#[specta::specta]
pub fn swap_preset(app: tauri::AppHandle) -> Result<(), AppError> {
    let app_data = app.state::<AppData>();

    app_data
        .state
        .send_modify_with(|state| -> anyhow::Result<()> {
            let choice = state
                .try_as_experiment_mut()
                .context("Must be in experiment")?
                .try_as_choice_mut()
                .context("Must be in a choice experiment")?;

            choice.swap_current_preset();

            Ok(())
        })?;

    Ok(())
}

#[derive(Serialize, Type)]
pub struct ResultWithExperiment {
    pub experiment_key: String,
    pub result: ExperimentResult,
}

/// Get all results
#[tauri::command]
#[specta::specta]
pub async fn get_results() -> Result<Vec<WithKey<ResultWithExperiment>>, AppError> {
    let mut all_results = Vec::new();

    // Get the results directory
    let results_dir = storage::data_folder()?.join("results");
    println!("Reading results from: {}", results_dir.display());

    // Create the directory if it doesn't exist
    if !results_dir.exists() {
        println!("Results directory does not exist, creating it");
        fs::create_dir_all(&results_dir)
            .await
            .context("Could not create results directory")?;
        return Ok(all_results); // Return empty results if directory was just created
    }

    // Read all experiment folders in the results directory
    let mut experiment_dirs = fs::read_dir(&results_dir)
        .await
        .context("Could not read results directory")?;

    // Iterate through each experiment folder
    while let Some(experiment_dir_entry) = experiment_dirs
        .next_entry()
        .await
        .context("Failed to read directory entry")?
    {
        if !experiment_dir_entry
            .file_type()
            .await
            .context("Could not get file type")?
            .is_dir()
        {
            continue; // Skip non-directory entries
        }

        // Get the experiment key from the folder name
        let experiment_key = experiment_dir_entry
            .file_name()
            .to_str()
            .context("Could not convert folder name to string")?
            .to_owned();

        println!("Processing experiment folder: {}", experiment_key);

        // Read all JSON files in this experiment folder
        let experiment_dir_path = experiment_dir_entry.path();
        let Ok(mut result_files) = fs::read_dir(&experiment_dir_path).await else {
            println!(
                "Could not read files in experiment directory: {}",
                experiment_dir_path.display()
            );
            continue;
        };

        // Process each file in the experiment folder
        while let Ok(Some(file_entry)) = result_files.next_entry().await {
            let file_path = file_entry.path();

            // Skip non-JSON files
            if file_path.extension().and_then(|ext| ext.to_str()) != Some("json") {
                continue;
            }

            let file_stem = match file_path.file_stem().and_then(|s| s.to_str()) {
                Some(stem) => stem.to_owned(),
                None => {
                    println!("Could not get file stem from path: {}", file_path.display());
                    continue;
                }
            };

            println!("Reading result file: {}", file_path.display());

            // Read the file content
            let file_content = match fs::read_to_string(&file_path).await {
                Ok(content) => content,
                Err(err) => {
                    println!("Error reading file {}: {}", file_path.display(), err);
                    continue;
                }
            };

            // First parse as generic JSON Value to avoid deserialization issues
            let json_value: Value = match serde_json::from_str(&file_content) {
                Ok(value) => value,
                Err(err) => {
                    println!("Error parsing JSON from {}: {}", file_path.display(), err);
                    continue;
                }
            };

            // Now try to convert to our ExperimentResult type
            let result = match serde_json::from_value::<ExperimentResult>(json_value.clone()) {
                Ok(result) => result,
                Err(err) => {
                    println!(
                        "Error deserializing experiment result from {}: {}",
                        file_path.display(),
                        err
                    );
                    continue;
                }
            };

            all_results.push(WithKey {
                key: file_stem,
                value: ResultWithExperiment {
                    experiment_key: experiment_key.clone(),
                    result,
                },
            });
        }
    }

    println!("Found {} result(s)", all_results.len());
    Ok(all_results)
}

/// Delete a result file
///
/// * `key` - The unique identifier of the result to delete
/// * `experiment_key` - The experiment key that the result belongs to
#[tauri::command]
#[specta::specta]
pub async fn delete_result(key: String, experiment_key: String) -> Result<(), AppError> {
    storage::delete_file(&key, Folder::Results { experiment_key }).await?;

    Ok(())
}
