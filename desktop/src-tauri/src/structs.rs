use chrono::{DateTime, Local};
use serde::{Deserialize, Serialize};
use specta::Type;
use std::collections::HashMap;

#[derive(Deserialize, Serialize, Type, Clone, Copy, Debug, PartialEq)]
pub enum RenderParameter {
    Hue,
    Smoothness,
    Metallic,
    Emission,
}

#[derive(Serialize, Deserialize, Type, Clone, Debug, Default, PartialEq)]
pub struct RenderParameters {
    pub hue: f32,
    pub smoothness: f32,
    pub metallic: f32,
    pub emission: f32,
}

impl RenderParameters {
    pub fn get(&self, param: RenderParameter) -> f32 {
        match param {
            RenderParameter::Hue => self.hue,
            RenderParameter::Smoothness => self.smoothness,
            RenderParameter::Metallic => self.metallic,
            RenderParameter::Emission => self.emission,
        }
    }

    pub fn set(&mut self, param: RenderParameter, value: f32) {
        match param {
            RenderParameter::Hue => self.hue = value,
            RenderParameter::Smoothness => self.smoothness = value,
            RenderParameter::Metallic => self.metallic = value,
            RenderParameter::Emission => self.emission = value,
        }
    }
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct Choice {
    pub a: String,
    pub b: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, Type, PartialEq)]
#[serde(tag = "experiment_type")]
pub enum ExperimentAnswer {
    #[serde(rename = "choice")]
    Choice,

    #[serde(rename = "rating")]
    Rating { value: u8 },
}

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct Preset {
    pub name: String,
    pub parameters: RenderParameters,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct OutcomeChoice {
    pub a: String,
    pub b: String,
    pub selected: String,
    pub time: DateTime<Local>,
    pub duration_on_a: f64,
    pub duration_on_b: f64,
    pub duration: f64,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct OutcomeRating {
    pub preset: String,
    pub rank: u8,
    pub time: DateTime<Local>,
    pub duration: f64,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
#[serde(tag = "experiment_type")]
pub enum ExperimentType {
    /// Rating between 1-5
    #[serde(rename = "rating")]
    Rating { order: Vec<String> },

    /// Choose between two options
    #[serde(rename = "choice")]
    Choice { choices: Vec<Choice> },
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum UnityExperimentType {
    #[serde(rename = "choice")]
    Choice,
    #[serde(rename = "rating")]
    Rating,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ExperimentPrompt {
    pub experiment_type: UnityExperimentType,
    pub parameters: RenderParameters,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
#[serde(tag = "experiment_type")]
pub enum ExperimentResultType {
    #[serde(rename = "rating")]
    Rating { ratings: Vec<OutcomeRating> },
    #[serde(rename = "choice")]
    Choice { choices: Vec<OutcomeChoice> },
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct Experiment {
    #[serde(flatten)]
    pub experiment_type: ExperimentType,
    pub name: String,
    pub presets: HashMap<String, Preset>,
}

#[derive(Serialize, Deserialize, Type)]
pub struct CreateExperiment {
    #[serde(flatten)]
    pub experiment_type: ExperimentType,
    pub name: String,
    pub presets: Vec<String>,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct ExperimentResult {
    #[serde(flatten)]
    pub experiment_type: ExperimentResultType,
    pub name: String,
    pub time: DateTime<Local>,
    pub observer_id: u32,
    pub note: String,
    pub presets: HashMap<String, Preset>,
}

#[derive(Debug, Clone, Copy, Serialize, Deserialize, Type, PartialEq, Eq)]
pub enum CurrentPreset {
    A,
    B,
}

impl ExperimentResult {
    //TODO lage funksjon for å avslutte eksperimentet. Når man avslutter så lagres det til fil i korresponderende folder
    pub fn new(experiment: &Experiment, observer_id: u32, note: String) -> Self {
        Self {
            experiment_type: match experiment.experiment_type {
                ExperimentType::Choice { .. } => ExperimentResultType::Choice {
                    choices: Vec::new(),
                },
                ExperimentType::Rating { .. } => ExperimentResultType::Rating {
                    ratings: Vec::new(),
                },
            },
            name: experiment.name.clone(),
            observer_id,
            time: Local::now(),
            note,
            presets: experiment.presets.clone(),
        }
    }
}
