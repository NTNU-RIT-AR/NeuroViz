use std::{collections::HashMap, ops::Deref, sync::Arc};

use crate::{
    api::{storage, utils},
    consts::Folder,
    structs::{
        Choice, CurrentPreset, Experiment, ExperimentAnswer, ExperimentResult,
        ExperimentResultType, ExperimentType, OutcomeChoice, OutcomeRating, ParameterValues,
        Preset,
    },
};
use anyhow::{bail, Context};
use chrono::{prelude::Local, DateTime};
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;
use tauri::async_runtime::block_on;
use tokio::sync::watch;

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct SharedExperiment {
    pub name: String,
    pub presets: HashMap<String, Preset>,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct ChoiceExperiment {
    #[serde(flatten)]
    pub shared: SharedExperiment,

    pub choices: Vec<Choice>,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct RatingExperiment {
    #[serde(flatten)]
    pub shared: SharedExperiment,

    pub order: Vec<String>,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct SharedExperimentResult {
    pub name: String,
    pub time: DateTime<Local>,
    pub observer_id: u32,
    pub note: String,
    pub presets: HashMap<String, Preset>,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct ChoiceExperimentResult {
    #[serde(flatten)]
    pub shared: SharedExperimentResult,

    pub choices: Vec<OutcomeChoice>,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct RatingExperimentResult {
    #[serde(flatten)]
    pub shared: SharedExperimentResult,

    pub ratings: Vec<OutcomeRating>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Type)]
pub struct SharedExperimentState {
    pub experiment_key: String,
    pub result_key: String,
    pub current_index: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize, Type)]
pub struct ChoiceExperimentState {
    #[serde(flatten)]
    pub shared: SharedExperimentState,
    pub experiment: ChoiceExperiment,
    pub result: ChoiceExperimentResult,
    pub current_preset: CurrentPreset,
}

impl ChoiceExperimentState {
    pub fn get_current_preset_key(&self) -> String {
        let choice = &self.experiment.choices[self.shared.current_index as usize];

        match self.current_preset {
            CurrentPreset::A => choice.a.clone(),
            CurrentPreset::B => choice.b.clone(),
        }
    }

    pub fn get_current_preset(&self) -> Preset {
        let preset_key = self.get_current_preset_key();
        let preset = self.experiment.shared.presets[&preset_key].clone();

        preset
    }

    pub fn swap_current_preset(&mut self) {
        match self.current_preset {
            CurrentPreset::A => self.current_preset = CurrentPreset::B,
            CurrentPreset::B => self.current_preset = CurrentPreset::A,
        };
    }

    pub fn answer(&mut self) -> anyhow::Result<bool> {
        if self.is_done() {
            return Ok(true);
        }

        let selected_preset_key = self.get_current_preset_key();

        let choice = &self.experiment.choices[self.shared.current_index as usize];

        let outcome = OutcomeChoice {
            a: choice.a.clone(),
            b: choice.b.clone(),
            selected: selected_preset_key,
            time: Local::now(),
            // TODO duration
            duration_on_a: 0.0,
            duration_on_b: 0.0,
            duration: 0.0,
        };

        self.result.choices.push(outcome);

        self.shared.current_index += 1;
        let is_done = self.is_done();

        Ok(is_done)
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, Type)]
pub struct RatingExperimentState {
    #[serde(flatten)]
    pub shared: SharedExperimentState,
    pub experiment: RatingExperiment,
    pub result: RatingExperimentResult,
}

impl RatingExperimentState {
    pub fn get_current_preset_key(&self) -> String {
        let preset_key = self.experiment.order[self.shared.current_index as usize].clone();

        preset_key
    }

    pub fn get_current_preset(&self) -> Preset {
        let preset_key = self.get_current_preset_key();
        let preset = self.experiment.shared.presets[&preset_key].clone();

        preset
    }

    pub fn is_done() {
        let is_done = order.len() == self.current_index as usize + 1;

        is_done
    }

    pub fn answer(&mut self, value: u8) -> anyhow::Result<bool> {
        if self.is_done() {
            return Ok(true);
        }

        let is_first_prompt = self.shared.current_index == 0;
        let duration = match is_first_prompt {
            true => utils::get_duration_since(self.experiment_result.time),
            false => utils::get_duration_since(ratings[self.current_index as usize - 1].time),
        };

        let outcome = OutcomeRating {
            preset: order[self.current_index as usize].clone(),
            rank: value,
            time: Local::now(),
            duration,
        };

        ratings.push(outcome);

        self.shared.current_index += 1;
        let is_done = self.is_done();

        Ok(is_done)
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, Type, EnumTryAs)]
#[serde(tag = "experiment_type")]
pub enum ExperimentState {
    #[serde(rename = "rating")]
    Rating(RatingExperimentState),
    #[serde(rename = "choice")]
    Choice(ChoiceExperimentState),
}

// impl ExperimentState {
//     pub fn new(
//         experiment_key: String,
//         result_key: String,
//         experiment: Experiment,
//         experiment_result: ExperimentResult,
//     ) -> Self {
//         let current_index = 0;
//         let choice_current_preset = CurrentPreset::A;

//         Self {
//             experiment_key,
//             result_key,
//             experiment,
//             experiment_result,
//             current_index,
//             choice_current_preset,
//         }
//     }
// }

impl Deref for ExperimentState {
    type Target = SharedExperimentState;

    fn deref(&self) -> &Self::Target {
        match self {
            ExperimentState::Rating(state) => &state.shared,
            ExperimentState::Choice(state) => &state.shared,
        }
    }
}

impl ExperimentState {
    pub fn get_current_preset_key(&self) -> String {
        match self {
            ExperimentState::Rating(state) => state.get_current_preset_key(),
            ExperimentState::Choice(state) => state.get_current_preset_key(),
        }
    }

    pub fn get_current_preset(&self) -> Preset {
        match self {
            ExperimentState::Rating(state) => state.get_current_preset(),
            ExperimentState::Choice(state) => state.get_current_preset(),
        }
    }

    pub fn answer(&mut self, experiment_answer: ExperimentAnswer) -> anyhow::Result<bool> {
        let is_done = match experiment_answer {
            ExperimentAnswer::Choice => {
                let choice_state = self
                    .try_as_choice_mut()
                    .context("Failed to get choice state")?;

                choice_state.answer()
            }
            ExperimentAnswer::Rating { value } => {
                let rating_state = self
                    .try_as_rating_mut()
                    .context("Failed to get rating state")?;

                rating_state.answer(value)?
            }
        };

        Ok(is_done)
    }
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
    pub fn swap_current_preset(&mut self) -> anyhow::Result<()> {
        let AppState::Experiment(experiment_state) = self else {
            bail!("Not in experiment mode");
        };

        experiment_state.swap_current_preset();

        Ok(())
    }

    pub fn answer_experiment(&mut self, experiment_answer: ExperimentAnswer) -> anyhow::Result<()> {
        let AppState::Experiment(experiment_state) = self else {
            bail!("Not in experiment mode");
        };

        let is_done = experiment_state.answer(experiment_answer)?;

        // TODO: Move this out of AppState to keep it pure
        if is_done {
            // Experiment is over, save the result
            println!("Experiment is done, saving result");

            let result_name = format!(
                "{}-{}",
                Local::now().format("%Y-%m-%d-%H.%M.%S"),
                experiment_state.result_key,
            );

            block_on(storage::create_file(
                result_name,
                &experiment_state.experiment_result,
                Folder::Results {
                    experiment_key: experiment_state.experiment_key.clone(),
                },
            ))?;

            *self = AppState::LiveView(ParameterValues::default());
        }

        Ok(())
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
