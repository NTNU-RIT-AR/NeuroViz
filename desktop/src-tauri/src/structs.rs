use chrono::{DateTime, Local};
use serde::{Deserialize, Serialize};
use specta::Type;
use std::collections::HashMap;

#[derive(Deserialize, Serialize, Type, Clone, Copy, Debug, PartialEq, Hash, Eq)]
pub enum ParameterKey {
    #[serde(rename = "transparency")]
    Transparency,
    #[serde(rename = "see_through")]
    SeeThrough,
    #[serde(rename = "outline")]
    Outline,
    #[serde(rename = "smoothness")]
    Smoothness,
}

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct Parameter {
    key: ParameterKey,
    name: String,
}

impl Parameter {
    pub fn all() -> Vec<Parameter> {
        [
            Parameter {
                key: ParameterKey::Transparency,
                name: "Transparency".to_owned(),
            },
            Parameter {
                key: ParameterKey::SeeThrough,
                name: "See through".to_owned(),
            },
            Parameter {
                key: ParameterKey::Outline,
                name: "Outline".to_owned(),
            },
            Parameter {
                key: ParameterKey::Smoothness,
                name: "Smoothness".to_owned(),
            },
        ]
        .to_vec()
    }
}

#[derive(Deserialize, Serialize, Type, Default, Clone, Debug, PartialEq)]
pub struct ParameterValues {
    pub transparency: f32,
    pub see_through: f32,
    pub outline: f32,
    pub smoothness: f32,
}

impl ParameterValues {
    pub fn get(&self, param: ParameterKey) -> f32 {
        match param {
            ParameterKey::Transparency => self.transparency,
            ParameterKey::SeeThrough => self.see_through,
            ParameterKey::Outline => self.outline,
            ParameterKey::Smoothness => self.smoothness,
        }
    }

    pub fn set(&mut self, param: ParameterKey, value: f32) {
        match param {
            ParameterKey::Transparency => self.transparency = value,
            ParameterKey::SeeThrough => self.see_through = value,
            ParameterKey::Outline => self.outline = value,
            ParameterKey::Smoothness => self.smoothness = value,
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
    pub parameters: ParameterValues,
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
    pub parameters: ParameterValues,
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
    pub name: String,
    pub presets: HashMap<String, Preset>,
    #[serde(flatten)]
    pub experiment_type: ExperimentType,
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
    pub name: String,
    pub time: DateTime<Local>,
    pub observer_id: u32,
    pub note: String,
    pub presets: HashMap<String, Preset>,
    #[serde(flatten)]
    pub experiment_type: ExperimentResultType,
}

#[derive(Debug, Clone, Copy, Serialize, Deserialize, Type, PartialEq, Eq)]
pub enum CurrentPreset {
    A,
    B,
}

impl ExperimentResult {
    //TODO lage funksjon for å avslutte eksperimentet. Når man avslutter så lagres det til fil i korresponderende folder
    pub fn new(experiment: &Experiment, name: String, observer_id: u32, note: String) -> Self {
        Self {
            experiment_type: match experiment.experiment_type {
                ExperimentType::Choice { .. } => ExperimentResultType::Choice {
                    choices: Vec::new(),
                },
                ExperimentType::Rating { .. } => ExperimentResultType::Rating {
                    ratings: Vec::new(),
                },
            },
            name,
            observer_id,
            time: Local::now(),
            note,
            presets: experiment.presets.clone(),
        }
    }
}
