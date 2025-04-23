use std::{ops::Deref, path::PathBuf};

use crate::{
    data::{
        experiment::{ChoiceExperiment, CurrentPreset, ExperimentAnswer, RatingExperiment},
        experiment_result::{ChoiceExperimentResult, ExperimentResult, RatingExperimentResult},
        preset::Preset,
    },
    storage::{self, Folder},
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

#[cfg(test)]
mod tests {
    use std::collections::HashMap;
    use super::*;
    use crate::data::parameters::ParameterValues;
    
    // Helper function to create a test rating experiment and result
    fn create_test_rating_experiment() -> (RatingExperiment, RatingExperimentResult) {
        // Create sample parameter values
        let parameters = ParameterValues {
            transparency: 0.5,
            see_through: 0.3,
            outline: 0.7,
            smoothness: 0.8,
        };
        
        // Create preset map
        let mut presets = HashMap::new();
        presets.insert(
            "preset1".to_string(),
            Preset {
                name: "Preset 1".to_string(),
                parameters: parameters.clone(),
            },
        );
        presets.insert(
            "preset2".to_string(),
            Preset {
                name: "Preset 2".to_string(),
                parameters: parameters.clone(),
            },
        );
        
        // Create experiment with fixed order
        let experiment = RatingExperiment {
            shared: crate::data::experiment::SharedExperiment {
                name: "Test Experiment".to_string(),
                presets: presets.clone(),
            },
            order: vec!["preset1".to_string(), "preset2".to_string()],
        };
        
        // Create initial result
        let now = Local::now();
        let result = RatingExperimentResult {
            shared: crate::data::experiment_result::SharedExperimentResult {
                name: "Test Result".to_string(),
                time: now,
                observer_id: 1,
                note: "Test note".to_string(),
                presets,
            },
            ratings: vec![],
        };
        
        (experiment, result)
    }
    
    // Helper function to create a test choice experiment and result
    fn create_test_choice_experiment() -> (ChoiceExperiment, ChoiceExperimentResult) {
        // Create sample parameter values
        let parameters = ParameterValues {
            transparency: 0.5,
            see_through: 0.3,
            outline: 0.7,
            smoothness: 0.8,
        };
        
        // Create preset map
        let mut presets = HashMap::new();
        presets.insert(
            "preset1".to_string(),
            Preset {
                name: "Preset 1".to_string(),
                parameters: parameters.clone(),
            },
        );
        presets.insert(
            "preset2".to_string(),
            Preset {
                name: "Preset 2".to_string(),
                parameters: parameters.clone(),
            },
        );
        
        // Create choices
        let choices = vec![
            crate::data::experiment::Choice {
                a: "preset1".to_string(),
                b: "preset2".to_string(),
            },
        ];
        
        // Create experiment
        let experiment = ChoiceExperiment {
            shared: crate::data::experiment::SharedExperiment {
                name: "Test Experiment".to_string(),
                presets: presets.clone(),
            },
            choices,
        };
        
        // Create initial result
        let now = Local::now();
        let result = ChoiceExperimentResult {
            shared: crate::data::experiment_result::SharedExperimentResult {
                name: "Test Result".to_string(),
                time: now,
                observer_id: 1,
                note: "Test note".to_string(),
                presets,
            },
            choices: vec![],
        };
        
        (experiment, result)
    }
    
    #[test]
    fn test_new_rating() {
        let (experiment, result) = create_test_rating_experiment();
        let state = ExperimentState::new_rating(
            "exp1".to_string(),
            "res1".to_string(),
            experiment.clone(),
            result.clone(),
        );
        
        // Verify the state was created correctly
        match state {
            ExperimentState::Rating(rating_state) => {
                assert_eq!(rating_state.shared.experiment_key, "exp1");
                assert_eq!(rating_state.shared.result_key, "res1");
                assert_eq!(rating_state.shared.current_index, 0);
                assert_eq!(rating_state.experiment.order, experiment.order);
                assert_eq!(rating_state.result.ratings.len(), 0);
            }
            _ => panic!("Expected Rating experiment state"),
        }
    }
    
    #[test]
    fn test_new_choice() {
        let (experiment, result) = create_test_choice_experiment();
        let state = ExperimentState::new_choice(
            "exp1".to_string(),
            "res1".to_string(),
            experiment.clone(),
            result.clone(),
        );
        
        // Verify the state was created correctly
        match state {
            ExperimentState::Choice(choice_state) => {
                assert_eq!(choice_state.shared.experiment_key, "exp1");
                assert_eq!(choice_state.shared.result_key, "res1");
                assert_eq!(choice_state.shared.current_index, 0);
                assert_eq!(choice_state.experiment.choices.len(), experiment.choices.len());
                assert_eq!(choice_state.result.choices.len(), 0);
                assert_eq!(choice_state.current_preset, CurrentPreset::A);
            }
            _ => panic!("Expected Choice experiment state"),
        }
    }
    
    #[test]
    fn test_get_current_preset_key() {
        // Test with rating experiment
        let (experiment, result) = create_test_rating_experiment();
        let state = ExperimentState::new_rating(
            "exp1".to_string(),
            "res1".to_string(),
            experiment,
            result,
        );
        
        assert_eq!(state.get_current_preset_key(), "preset1");
    }
    
    #[test]
    fn test_get_current_preset() {
        // Test with rating experiment
        let (experiment, result) = create_test_rating_experiment();
        let state = ExperimentState::new_rating(
            "exp1".to_string(),
            "res1".to_string(),
            experiment,
            result,
        );
        
        let preset = state.get_current_preset();
        assert_eq!(preset.name, "Preset 1");
    }
    
    #[test]
    fn test_answer_rating() {
        let (experiment, result) = create_test_rating_experiment();
        let mut state = ExperimentState::new_rating(
            "exp1".to_string(),
            "res1".to_string(),
            experiment,
            result,
        );
        
        // Answer with rating value
        let is_done = state.answer(ExperimentAnswer::Rating { value: 4 }).unwrap();
        
        // Not done yet (we have 2 presets)
        assert!(!is_done);
        
        // Check that the state advanced
        match &state {
            ExperimentState::Rating(rating_state) => {
                assert_eq!(rating_state.shared.current_index, 1);
                assert_eq!(rating_state.result.ratings.len(), 1);
                assert_eq!(rating_state.result.ratings[0].preset, "preset1");
                assert_eq!(rating_state.result.ratings[0].rank, 4);
            }
            _ => panic!("Expected Rating experiment state"),
        }
    }
}
