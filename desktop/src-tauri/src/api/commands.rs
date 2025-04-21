use anyhow::Context;
use chrono::Local;
use local_ip_address::local_ip;
use serde::{Deserialize, Serialize};
use slug::slugify;
use specta::Type;
use std::collections::HashMap;
use tauri::Manager;
use tauri_specta::Event;

use crate::{
    data::{
        experiment::{
            ChoiceExperiment, CreateExperiment, CreateExperimentType, Experiment, ExperimentAnswer,
            RatingExperiment,
        },
        experiment_result::{ChoiceExperimentResult, RatingExperimentResult},
        parameters::{Parameter, ParameterValues},
        preset::Preset,
    },
    extensions::WatchSenderExt,
    state::{experiment_state::ExperimentState, AppData, AppState},
};

use super::{
    command_error::AppError,
    events::ResultSavedEvent,
    storage::{self, Folder},
};

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
    Parameter::all()
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
pub async fn create_experiment(experiment_init_data: CreateExperiment) -> Result<String, AppError> {
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

    //Create and write to JSON file
    storage::create_file(&experiment_key, &experiment, Folder::Experiments).await?;
    //TODO: Kan eventuelt returnere det nye eksperimentet sånn det kan vises på frontend som en slags bekreftelse
    Ok(String::from("Experiment created successfully"))
}

/// Delete an experiment
#[tauri::command]
#[specta::specta]
pub async fn delete_experiment(key: String) -> Result<(), AppError> {
    storage::delete_file(&key, Folder::Experiments).await?;

    Ok(())
}

/// Get all parameters in live view
#[tauri::command]
#[specta::specta]
pub fn get_live_parameters(app: tauri::AppHandle) -> Result<ParameterValues, AppError> {
    let app_data = app.state::<AppData>();
    let app_state = app_data.state.borrow();

    let live_state = app_state
        .try_as_live_view_ref()
        .context("Must be in live mode")?;

    Ok(live_state.clone())
}

/// Set all parameters in live view
#[tauri::command]
#[specta::specta]
pub fn set_live_parameters(
    app: tauri::AppHandle,
    parameters: ParameterValues,
) -> Result<(), AppError> {
    let app_data = app.state::<AppData>();

    app_data.state.send_modify_with(|app_state| {
        let live_state = app_state
            .try_as_live_view_mut()
            .context("Must be in live mode")?;

        *live_state = parameters;

        Ok(())
    })
}

#[tauri::command]
#[specta::specta]
pub async fn start_experiment(
    app: tauri::AppHandle,
    experiment_key: String,
    result_name: String,
    obeserver_id: u32,
    note: String,
) -> Result<(), AppError> {
    let result_key = slugify(&result_name);

    let experiment = storage::read_file::<Experiment>(&experiment_key, Folder::Experiments).await?;

    let time = Local::now();

    let experiment_state = match experiment {
        Experiment::Rating(rating_experiment) => {
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

        Experiment::Choice(choice_experiment) => {
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

    let experiment_is_done = app_data
        .state
        .send_modify_with(|state| state.answer_experiment(answer))?;

    if experiment_is_done {
        let experiment_state = app_data
            .state
            .send_replace(AppState::LiveView(Default::default()))
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
