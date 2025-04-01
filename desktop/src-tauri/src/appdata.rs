use crate::{
    api::{storage, utils},
    consts::Folder,
    structs::{
        CurrentPreset, Experiment, ExperimentAnswer, ExperimentResult, ExperimentResultType,
        ExperimentType, OutcomeChoice, OutcomeRating, ParameterValues, Preset,
    },
};
use chrono::prelude::Local;
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
    pub fn get_current_preset_key(&self) -> String {
        let Self {
            experiment,
            current_index,
            choice_current_preset,
            ..
        } = self;

        let preset_key = match &experiment.experiment_type {
            ExperimentType::Choice { choices } => {
                let choice = &choices[*current_index as usize];

                match choice_current_preset {
                    CurrentPreset::A => choice.a.clone(),
                    CurrentPreset::B => choice.b.clone(),
                }
            }

            ExperimentType::Rating { order } => order[*current_index as usize].clone(),
        };

        preset_key
    }

    pub fn get_current_preset(&self) -> Preset {
        let preset_key = self.get_current_preset_key();
        let preset = self.experiment.presets[&preset_key].clone();

        preset
    }

    pub fn swap_current_preset(&mut self) {
        match self.choice_current_preset {
            CurrentPreset::A => self.choice_current_preset = CurrentPreset::B,
            CurrentPreset::B => self.choice_current_preset = CurrentPreset::A,
        };
    }

    pub fn answer(&mut self, experiment_answer: ExperimentAnswer) -> Result<bool, String> {
        let is_done = match experiment_answer {
            ExperimentAnswer::Choice => self.answer_choice()?,
            ExperimentAnswer::Rating { value } => self.answer_rating(value)?,
        };

        self.current_index += 1;

        Ok(is_done)
    }

    fn answer_rating(&mut self, value: u8) -> Result<bool, String> {
        let ExperimentType::Rating { order } = &mut self.experiment.experiment_type else {
            return Err("Not a rating experiment".to_string());
        };

        let ExperimentResultType::Rating { ratings } = &mut self.experiment_result.experiment_type
        else {
            return Err("Not a rating experiment".to_string());
        };

        let is_first_prompt = self.current_index == 0;
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

        let is_done = order.len() == self.current_index as usize + 1;

        Ok(is_done)
    }

    fn answer_choice(&mut self) -> Result<bool, String> {
        let selected_preset_key = self.get_current_preset_key();

        let ExperimentType::Choice {
            choices: choices_experiment,
        } = &mut self.experiment.experiment_type
        else {
            // Not a choice experiment, do nothing
            return Err("Not a choice experiment".to_string());
        };

        let ExperimentResultType::Choice { choices } = &mut self.experiment_result.experiment_type
        else {
            // Not a choice experiment, do nothing
            return Err("Not a choice experiment".to_string());
        };

        let choice = &choices_experiment[self.current_index as usize];

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

        choices.push(outcome);

        let is_done = choices_experiment.len() == self.current_index as usize + 1;

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
    pub fn swap_current_preset(&mut self) -> Result<(), String> {
        let AppState::Experiment(experiment_state) = self else {
            return Err("Not in experiment mode".to_string());
        };

        experiment_state.swap_current_preset();

        Ok(())
    }

    pub fn answer_experiment(&mut self, experiment_answer: ExperimentAnswer) -> Result<(), String> {
        let AppState::Experiment(experiment_state) = self else {
            return Err("Not in experiment mode".to_string());
        };

        let is_done = experiment_state.answer(experiment_answer)?;

        if is_done {
            // Experiment is over, save the result
            println!("Experiment is done, saving result");

            let result_name = format!(
                "{}-{}",
                Local::now().format("%Y-%m-%d_%H-%M-%S"),
                experiment_state.experiment.name,
            );

            storage::create_and_write_to_json_file(
                &experiment_state.experiment_result,
                Folder::Results,
                result_name,
            )?;

            *self = AppState::LiveView(ParameterValues::default());
        }

        Ok(())
    }
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
