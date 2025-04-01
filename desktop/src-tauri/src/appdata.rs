use crate::structs::{
    CurrentPreset, Experiment, ExperimentResult, ExperimentType, Preset, RenderParameters,
};
use futures_signals::signal::Mutable;
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;

#[derive(Debug, Clone, Serialize, Deserialize, Type)]
pub struct ExperimentState {
    pub experiment: Experiment,
    pub experiment_result: ExperimentResult,
    pub current_index: u32,
    pub choice_current_preset: CurrentPreset,
}

impl ExperimentState {
    pub fn new(experiment: Experiment, experiment_result: ExperimentResult) -> Self {
        let current_index = 0;
        let choice_current_preset = CurrentPreset::A;

        Self {
            experiment,
            experiment_result,
            current_index,
            choice_current_preset,
        }
    }
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
                let choice = &choices[*current_index as usize];

                match choice_current_preset {
                    CurrentPreset::A => experiment.presets[&choice.a].clone(),
                    CurrentPreset::B => experiment.presets[&choice.b].clone(),
                }
            }

            ExperimentType::Rating { order } => {
                let preset_name = &order[*current_index as usize];

                experiment.presets[preset_name].clone()
            }
        };

        preset
    }

    pub fn swap_current_preset(&mut self) {
        match self.choice_current_preset {
            CurrentPreset::A => self.choice_current_preset = CurrentPreset::B,
            CurrentPreset::B => self.choice_current_preset = CurrentPreset::A,
        };
    }
}

#[derive(Debug, Clone, EnumTryAs, Serialize, Deserialize, Type)]
pub enum AppState {
    LiveView(RenderParameters),
    Experiment(ExperimentState),
}

/// A handle to all the state of the app.
#[derive(Clone)]
pub struct AppData {
    pub state: Mutable<AppState>,
}

impl AppData {
    pub fn new(state: AppState) -> Self {
        Self {
            state: Mutable::new(state),
        }
    }
}
