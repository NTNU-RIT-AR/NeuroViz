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
