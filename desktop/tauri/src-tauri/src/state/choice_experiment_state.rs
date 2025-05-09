use chrono::Local;
use serde::{Deserialize, Serialize};
use specta::Type;

use crate::data::{
    experiment::{ChoiceExperiment, CurrentPreset},
    experiment_result::{ChoiceExperimentResult, OutcomeChoice},
    preset::Preset,
};

use super::{experiment_state::SharedExperimentState, get_duration_since};

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

    pub fn is_done(&self) -> bool {
        let is_done = self.experiment.choices.len() == self.shared.current_index as usize;

        is_done
    }

    pub fn answer(&mut self) -> anyhow::Result<bool> {
        if self.is_done() {
            return Ok(true);
        }

        let selected_preset_key = self.get_current_preset_key();

        let choice = &self.experiment.choices[self.shared.current_index as usize];

        let is_first_prompt = self.shared.current_index == 0;
        let duration = match is_first_prompt {
            true => get_duration_since(self.result.shared.time),
            false => {
                get_duration_since(self.result.choices[self.shared.current_index as usize - 1].time)
            }
        };

        let outcome = OutcomeChoice {
            a: choice.a.clone(),
            b: choice.b.clone(),
            selected: selected_preset_key,
            time: Local::now(),
            duration,
        };

        self.result.choices.push(outcome);

        self.shared.current_index += 1;
        let is_done = self.is_done();

        Ok(is_done)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashMap;

    use chrono::Local;
    use neuroviz::parameters::ParameterValues;

    use crate::data::{
        experiment::{Choice, ChoiceExperiment, CurrentPreset, SharedExperiment},
        experiment_result::{ChoiceExperimentResult, SharedExperimentResult},
        preset::Preset,
    };
    use crate::state::experiment_state::SharedExperimentState;

    fn create_test_state(
        current_index: u32,
        current_preset: CurrentPreset,
        choices: Vec<Choice>,
    ) -> ChoiceExperimentState {
        // Create test presets
        let mut presets = HashMap::new();

        // Create preset A
        let preset_a = Preset {
            name: "Preset A".to_string(),
            parameters: ParameterValues {
                transparency: 0.5,
                glow: 0.7,
                smoothness: 0.2,
                emission: 0.3,
                light_intensity: 0.5,
                light_temperature: 0.,
            },
        };

        // Create preset B
        let preset_b = Preset {
            name: "Preset B".to_string(),
            parameters: ParameterValues {
                transparency: 0.2,
                glow: 0.4,
                smoothness: 0.6,
                emission: 0.9,
                light_intensity: 0.8,
                light_temperature: 0.,
            },
        };

        // Add presets to hashmap
        presets.insert("preset_a".to_string(), preset_a);
        presets.insert("preset_b".to_string(), preset_b);

        // Create choice experiment
        let experiment = ChoiceExperiment {
            shared: SharedExperiment {
                name: "Test Experiment".to_string(),
                presets,
            },
            choices,
        };

        // Create shared experiment result
        let shared_result = SharedExperimentResult {
            name: "Test Result".to_string(),
            time: Local::now(),
            observer_id: 1,
            note: "Test note".to_string(),
            presets: experiment.shared.presets.clone(),
        };

        // Create choice experiment result
        let result = ChoiceExperimentResult {
            shared: shared_result,
            choices: Vec::new(),
        };

        // Create choice experiment state
        ChoiceExperimentState {
            shared: SharedExperimentState {
                experiment_key: "test_experiment".to_string(),
                result_key: "test_result".to_string(),
                current_index,
                is_idle: false,
            },
            experiment,
            result,
            current_preset,
        }
    }

    #[test]
    fn test_get_current_preset_key() {
        // Create choices
        let choices = vec![
            Choice {
                a: "preset_a".to_string(),
                b: "preset_b".to_string(),
            },
            Choice {
                a: "preset_b".to_string(),
                b: "preset_a".to_string(),
            },
        ];

        // Test for CurrentPreset::A
        let state = create_test_state(0, CurrentPreset::A, choices.clone());
        assert_eq!(state.get_current_preset_key(), "preset_a");

        // Test for CurrentPreset::B
        let state = create_test_state(0, CurrentPreset::B, choices.clone());
        assert_eq!(state.get_current_preset_key(), "preset_b");

        // Test for second choice with CurrentPreset::A
        let state = create_test_state(1, CurrentPreset::A, choices.clone());
        assert_eq!(state.get_current_preset_key(), "preset_b");

        // Test for second choice with CurrentPreset::B
        let state = create_test_state(1, CurrentPreset::B, choices);
        assert_eq!(state.get_current_preset_key(), "preset_a");
    }

    #[test]
    fn test_get_current_preset() {
        // Create choices
        let choices = vec![Choice {
            a: "preset_a".to_string(),
            b: "preset_b".to_string(),
        }];

        // Test for CurrentPreset::A
        let state = create_test_state(0, CurrentPreset::A, choices.clone());
        let preset = state.get_current_preset();
        assert_eq!(preset.name, "Preset A");
        assert_eq!(preset.parameters.transparency, 0.5);

        // Test for CurrentPreset::B
        let state = create_test_state(0, CurrentPreset::B, choices);
        let preset = state.get_current_preset();
        assert_eq!(preset.name, "Preset B");
        assert_eq!(preset.parameters.transparency, 0.2);
    }

    #[test]
    fn test_swap_current_preset() {
        // Create choices
        let choices = vec![Choice {
            a: "preset_a".to_string(),
            b: "preset_b".to_string(),
        }];

        // Test swapping from A to B
        let mut state = create_test_state(0, CurrentPreset::A, choices.clone());
        state.swap_current_preset();
        assert!(matches!(state.current_preset, CurrentPreset::B));

        // Test swapping from B to A
        let mut state = create_test_state(0, CurrentPreset::B, choices);
        state.swap_current_preset();
        assert!(matches!(state.current_preset, CurrentPreset::A));
    }

    #[test]
    fn test_is_done() {
        // Create choices
        let choices = vec![
            Choice {
                a: "preset_a".to_string(),
                b: "preset_b".to_string(),
            },
            Choice {
                a: "preset_b".to_string(),
                b: "preset_a".to_string(),
            },
        ];

        // Test when not done (current_index < choices.len())
        let state = create_test_state(0, CurrentPreset::A, choices.clone());
        assert!(!state.is_done());

        let state = create_test_state(1, CurrentPreset::A, choices.clone());
        assert!(!state.is_done());

        // Test when done (current_index == choices.len())
        let state = create_test_state(2, CurrentPreset::A, choices);
        assert!(state.is_done());
    }

    #[test]
    fn test_answer() {
        // Create choices
        let choices = vec![
            Choice {
                a: "preset_a".to_string(),
                b: "preset_b".to_string(),
            },
            Choice {
                a: "preset_b".to_string(),
                b: "preset_a".to_string(),
            },
        ];

        // Test recording an answer when not done, with preset A
        let mut state = create_test_state(0, CurrentPreset::A, choices.clone());
        let is_done = state.answer().unwrap();

        // Should not be done yet
        assert!(!is_done);
        // Should increment current_index
        assert_eq!(state.shared.current_index, 1);
        // Should record outcome
        assert_eq!(state.result.choices.len(), 1);
        assert_eq!(state.result.choices[0].a, "preset_a");
        assert_eq!(state.result.choices[0].b, "preset_b");
        assert_eq!(state.result.choices[0].selected, "preset_a");

        // Test recording an answer for the second choice
        let is_done = state.answer().unwrap();

        // Should be done now
        assert!(is_done);
        // Should increment current_index
        assert_eq!(state.shared.current_index, 2);
        // Should record second outcome
        assert_eq!(state.result.choices.len(), 2);
        assert_eq!(state.result.choices[1].a, "preset_b");
        assert_eq!(state.result.choices[1].b, "preset_a");
        assert_eq!(state.result.choices[1].selected, "preset_b");

        // Test returning when already done
        let is_done = state.answer().unwrap();
        assert!(is_done);
        // Should not add more outcomes
        assert_eq!(state.result.choices.len(), 2);
    }
}
