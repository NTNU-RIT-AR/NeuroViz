use std::sync::Arc;

use anyhow::bail;
use chrono::{DateTime, Local};
use experiment_state::ExperimentState;
use neuroviz::{
    http_server::{ExperimentAnswer, ExperimentPrompt, UnityExperimentType, UnityState},
    parameters::ParameterValues,
};
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;
use tokio::sync::watch;

pub mod choice_experiment_state;
pub mod experiment_state;
pub mod rating_experiment_state;

pub fn get_duration_since(then: DateTime<Local>) -> f64 {
    Local::now()
        .signed_duration_since(then)
        .num_nanoseconds()
        .unwrap() as f64
        / 1_000_000_000.0
}

#[derive(Debug, Clone, EnumTryAs, Serialize, Deserialize, Type)]
#[serde(tag = "kind")]
pub enum AppState {
    #[serde(rename = "idle")]
    Idle,

    #[serde(rename = "live_view")]
    LiveView(ParameterValues),

    #[serde(rename = "experiment")]
    Experiment(ExperimentState),
}

impl AppState {
    /// Answer experiment, returns true if experiment is done
    #[must_use]
    pub fn answer_experiment(
        &mut self,
        experiment_answer: ExperimentAnswer,
    ) -> anyhow::Result<bool> {
        let AppState::Experiment(experiment_state) = self else {
            bail!("Not in experiment mode");
        };

        let is_done = experiment_state.answer(experiment_answer)?;

        Ok(is_done)
    }
}

impl From<AppState> for UnityState {
    fn from(app_state: AppState) -> Self {
        match app_state {
            AppState::Idle => UnityState::Idle,

            AppState::LiveView(parameters) => UnityState::Live { parameters },

            AppState::Experiment(experiment_state) if experiment_state.is_idle() => {
                UnityState::Idle
            }

            AppState::Experiment(experiment_state) => UnityState::Experiment {
                prompt: ExperimentPrompt {
                    experiment_type: match experiment_state {
                        ExperimentState::Choice { .. } => UnityExperimentType::Choice,
                        ExperimentState::Rating { .. } => UnityExperimentType::Rating,
                    },
                    parameters: experiment_state.get_current_preset().parameters,
                },
            },
        }
    }
}

/// A handle to all the state of the app.
#[derive(Clone)]
pub struct AppData {
    pub state: watch::Sender<AppState>,
    pub connected_clients: watch::Sender<usize>,
    pub secret: Arc<String>,
}

impl AppData {
    pub fn new(state: AppState, secret: String) -> Self {
        Self {
            state: watch::Sender::new(state),
            connected_clients: watch::Sender::new(0),
            secret: Arc::new(secret),
        }
    }
}
