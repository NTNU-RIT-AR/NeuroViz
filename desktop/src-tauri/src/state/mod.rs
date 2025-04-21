use std::sync::Arc;

use anyhow::bail;
use chrono::{DateTime, Local};
use experiment_state::ExperimentState;
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;
use tokio::sync::watch;

use crate::data::{experiment::ExperimentAnswer, parameters::ParameterValues};

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

/// A handle to all the state of the app.
#[derive(Clone)]
pub struct AppData {
    pub state: watch::Sender<AppState>,
    pub secret: Arc<String>,
}

impl AppData {
    pub fn new(state: AppState, secret: Arc<String>) -> Self {
        Self {
            state: watch::Sender::new(state),
            secret,
        }
    }
}
