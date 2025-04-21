use std::{ops::Deref, path::PathBuf};

use crate::{
    api::storage::{self, Folder},
    data::{
        experiment::{ChoiceExperiment, CurrentPreset, ExperimentAnswer, RatingExperiment},
        experiment_result::{ChoiceExperimentResult, ExperimentResult, RatingExperimentResult},
        preset::Preset,
    },
};
use anyhow::Context;
use chrono::prelude::Local;
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;

use super::{
    choice_experiment_state::ChoiceExperimentState, rating_experiment_state::RatingExperimentState,
};

#[derive(Debug, Clone, Serialize, Deserialize, Type)]
pub struct SharedExperimentState {
    pub experiment_key: String,
    pub result_key: String,
    pub current_index: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize, Type, EnumTryAs)]
#[serde(tag = "experiment_type")]
pub enum ExperimentState {
    #[serde(rename = "rating")]
    Rating(RatingExperimentState),
    #[serde(rename = "choice")]
    Choice(ChoiceExperimentState),
}

impl ExperimentState {
    pub fn new_rating(
        experiment_key: String,
        result_key: String,
        experiment: RatingExperiment,
        result: RatingExperimentResult,
    ) -> Self {
        Self::Rating(RatingExperimentState {
            shared: SharedExperimentState {
                experiment_key,
                result_key,
                current_index: 0,
            },
            experiment,
            result,
        })
    }

    pub fn new_choice(
        experiment_key: String,
        result_key: String,
        experiment: ChoiceExperiment,
        result: ChoiceExperimentResult,
    ) -> Self {
        Self::Choice(ChoiceExperimentState {
            shared: SharedExperimentState {
                experiment_key,
                result_key,
                current_index: 0,
            },
            current_preset: CurrentPreset::A,
            experiment,
            result,
        })
    }
}

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

                choice_state.answer()?
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

    /// Save the experiment result to a file, returns the file path
    pub async fn finish_experiment(self) -> anyhow::Result<PathBuf> {
        // Experiment is over, save the result
        println!("Experiment is done, saving result");

        let result_name = format!(
            "{}-{}",
            Local::now().format("%Y-%m-%d-%H.%M.%S"),
            self.result_key,
        );

        let experiment_key = self.experiment_key.clone();

        let result = match self {
            ExperimentState::Rating(rating_experiment_state) => {
                ExperimentResult::Rating(rating_experiment_state.result)
            }
            ExperimentState::Choice(choice_experiment_state) => {
                ExperimentResult::Choice(choice_experiment_state.result)
            }
        };

        let file_path =
            storage::create_file(&result_name, &result, Folder::Results { experiment_key }).await?;

        Ok(file_path)
    }
}
