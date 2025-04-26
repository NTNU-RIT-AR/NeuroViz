use chrono::Local;
use serde::{Deserialize, Serialize};
use specta::Type;

use crate::data::{
    experiment::RatingExperiment,
    experiment_result::{OutcomeRating, RatingExperimentResult},
    preset::Preset,
};

use super::{experiment_state::SharedExperimentState, get_duration_since};

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

    pub fn is_done(&self) -> bool {
        let is_done = self.experiment.order.len() == self.shared.current_index as usize;

        is_done
    }

    pub fn answer(&mut self, value: u8) -> anyhow::Result<bool> {
        if self.is_done() {
            return Ok(true);
        }

        let is_first_prompt = self.shared.current_index == 0;
        let duration = match is_first_prompt {
            true => get_duration_since(self.result.shared.time),
            false => {
                get_duration_since(self.result.ratings[self.shared.current_index as usize - 1].time)
            }
        };

        let outcome = OutcomeRating {
            preset: self.experiment.order[self.shared.current_index as usize].clone(),
            rank: value,
            time: Local::now(),
            duration,
        };

        self.result.ratings.push(outcome);

        self.shared.current_index += 1;
        let is_done = self.is_done();

        Ok(is_done)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    use neuroviz::parameters::ParameterValues;
    use std::collections::HashMap;

    // Helper function to create test data
    fn create_test_state() -> RatingExperimentState {
        // Create sample parameter values
        let parameters = ParameterValues {
            transparency: 0.5,
            glow: 0.7,
            smoothness: 0.8,
            emission: 0.9,
            light_intensity: 0.6,
            light_temperature: 0.,
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
        presets.insert(
            "preset3".to_string(),
            Preset {
                name: "Preset 3".to_string(),
                parameters: parameters,
            },
        );

        // Create experiment with fixed order
        let experiment = RatingExperiment {
            shared: crate::data::experiment::SharedExperiment {
                name: "Test Experiment".to_string(),
                presets,
            },
            order: vec![
                "preset1".to_string(),
                "preset2".to_string(),
                "preset3".to_string(),
            ],
        };

        // Create initial result
        let now = Local::now();
        let result = RatingExperimentResult {
            shared: crate::data::experiment_result::SharedExperimentResult {
                name: "Test Result".to_string(),
                time: now,
                observer_id: 1,
                note: "Test note".to_string(),
                presets: experiment.shared.presets.clone(),
            },
            ratings: vec![],
        };

        // Create shared experiment state
        let shared = SharedExperimentState {
            experiment_key: "experiment1".to_string(),
            result_key: "result1".to_string(),
            current_index: 0,
        };

        RatingExperimentState {
            shared,
            experiment,
            result,
        }
    }

    #[test]
    fn test_get_current_preset_key() {
        let mut state = create_test_state();

        // Test with initial index
        assert_eq!(state.get_current_preset_key(), "preset1");

        // Test after advancing index
        state.shared.current_index = 1;
        assert_eq!(state.get_current_preset_key(), "preset2");

        // Test with last index
        state.shared.current_index = 2;
        assert_eq!(state.get_current_preset_key(), "preset3");
    }

    #[test]
    fn test_get_current_preset() {
        let mut state = create_test_state();

        // Test with initial index
        let preset = state.get_current_preset();
        assert_eq!(preset.name, "Preset 1");

        // Test after advancing index
        state.shared.current_index = 1;
        let preset = state.get_current_preset();
        assert_eq!(preset.name, "Preset 2");
    }

    #[test]
    fn test_is_done() {
        let mut state = create_test_state();

        // Initial state - not done
        assert!(!state.is_done());

        // Middle state - not done
        state.shared.current_index = 1;
        assert!(!state.is_done());

        // Final state - not done
        state.shared.current_index = 2;
        assert!(!state.is_done());

        // Past final state - done
        state.shared.current_index = 3;
        assert!(state.is_done());
    }

    #[test]
    fn test_answer() {
        let mut state = create_test_state();

        // First answer - not done
        let is_done = state.answer(4).unwrap();
        assert!(!is_done);
        assert_eq!(state.shared.current_index, 1);
        assert_eq!(state.result.ratings.len(), 1);
        assert_eq!(state.result.ratings[0].preset, "preset1");
        assert_eq!(state.result.ratings[0].rank, 4);

        // Second answer - not done
        let is_done = state.answer(3).unwrap();
        assert!(!is_done);
        assert_eq!(state.shared.current_index, 2);
        assert_eq!(state.result.ratings.len(), 2);
        assert_eq!(state.result.ratings[1].preset, "preset2");
        assert_eq!(state.result.ratings[1].rank, 3);

        // Third answer - done
        let is_done = state.answer(5).unwrap();
        assert!(is_done);
        assert_eq!(state.shared.current_index, 3);
        assert_eq!(state.result.ratings.len(), 3);
        assert_eq!(state.result.ratings[2].preset, "preset3");
        assert_eq!(state.result.ratings[2].rank, 5);

        // Attempt to answer when done - should return true (done) without changing state
        let is_done = state.answer(2).unwrap();
        assert!(is_done);
        assert_eq!(state.shared.current_index, 3); // no change
        assert_eq!(state.result.ratings.len(), 3); // no change
    }
}
