use std::ops::Deref;

use crate::structs::{
    CurrentPreset, Experiment, ExperimentResult, ExperimentType, Preset, RenderParameters,
};

use futures_signals::signal::{Mutable, MutableLockMut, MutableLockRef};
use strum::EnumTryAs;
use tauri::Manager;

#[derive(Clone)]
pub struct ExperimentState {
    pub experiment_result: ExperimentResult,
    pub experiment: Experiment,
    pub current_index: usize,
    pub choice_current_preset: CurrentPreset,
}

impl ExperimentState {
    pub fn get_current_preset(&self) -> Preset {
        let Self {
            experiment,
            current_index,
            choice_current_preset,
            ..
        } = self;

        let preset = match &experiment.experiment_type {
            ExperimentType::Choice { choices } => {
                let choice = &choices[*current_index];

                match choice_current_preset {
                    CurrentPreset::A => experiment.presets[&choice.a].clone(),
                    CurrentPreset::B => experiment.presets[&choice.b].clone(),
                }
            }

            ExperimentType::Rating { order } => {
                let preset_name = &order[*current_index];

                experiment.presets[preset_name].clone()
            }
        };

        preset
    }

    pub fn swap_current_preset(&mut self) -> Result<(), String> {
        let Self {
            experiment,
            current_index,
            choice_current_preset,
            ..
        } = self;

        let ExperimentType::Choice { choices } = &experiment.experiment_type else {
            return Err("Experiment is not of type Choice".to_string());
        };

        let choice = &choices[*current_index];
        match choice_current_preset {
            CurrentPreset::A => *choice_current_preset = CurrentPreset::B,
            CurrentPreset::B => *choice_current_preset = CurrentPreset::A,
        };

        Ok(())
    }
}

// impl ExperimentState {
//     pub fn new() -> Self {
//         Self {
//             current_index: 0,
//             choice_current_preset: CurrentPreset::A,
//         }
//     }
// }

#[derive(Clone, EnumTryAs)]
pub enum AppState {
    Idle,
    LiveView(RenderParameters),
    Experiment(ExperimentState),
}

#[derive(Clone)]
pub struct AppData {
    pub state: Mutable<AppState>,
}

impl AppData {
    pub fn new() -> Self {
        AppData {
            state: Mutable::new(AppState::Idle),
        }
    }
}
