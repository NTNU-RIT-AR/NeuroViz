use crate::{api::utils, structs::{
    CurrentPreset, Experiment, ExperimentAnswer, ExperimentResult, ExperimentResultType, ExperimentType, OutcomeChoice, OutcomeRating, Preset, RenderParameters
}};
use futures_signals::signal::Mutable;
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;
use chrono::prelude::Local;
use slug::slugify;

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

    pub fn answer(&mut self, experiment_answer: ExperimentAnswer){
        match experiment_answer {
            ExperimentAnswer::Choice => self.answer_choice(),
            ExperimentAnswer::Rating { value } => self.answer_rating(value)
        }

        self.current_index += 1;
    }

    fn answer_rating(&mut self, value: u8) {
        if let ExperimentType::Rating {order} = &mut self.experiment.experiment_type {
                
            if let ExperimentResultType::Rating { ratings } = &mut self.experiment_result.experiment_type {
                let duration;

                if self.current_index > 0 {
                    duration = utils::get_duration_since(ratings[self.current_index - 1].time);
                } else {
                    duration = utils::get_duration_since(self.experiment_result.time);
                }

                let outcome = OutcomeRating {preset: order[self.current_index].clone(), rank: value, time: Local::now(), duration};
                
                ratings.push(outcome);

                //TODO handle when experiment is over
                if order.len() == self.current_index+1 {
                    
                } 
            }
        }
    }

    fn answer_choice(&mut self) {
        let selected_name = slugify(self.get_current_preset().name.clone());
    
        if let ExperimentType::Choice { choices: choices_experiment } = &mut self.experiment.experiment_type {
            if let ExperimentResultType::Choice { choices } = &mut self.experiment_result.experiment_type {
                
                let outcome = OutcomeChoice {
                    a: choices_experiment[self.current_index].a.clone(),
                    b: choices_experiment[self.current_index].b.clone(),
                    selected: selected_name,
                    time: Local::now(),
                    duration_on_a: 0.0,
                    duration_on_b: 0.0,
                    duration: 0.0,
                };
    
                choices.push(outcome);

                //TODO handle when experiment is over
                if choices_experiment.len() == self.current_index+1 {
                    
                } 
            }
        }
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
