use std::collections::HashMap;

use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;

use super::preset::Preset;

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct Choice {
    pub a: String,
    pub b: String,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
#[serde(tag = "experiment_type")]
pub enum CreateExperimentType {
    /// Rating between 1-5
    #[serde(rename = "rating")]
    Rating { order: Vec<String> },

    /// Choose between two options
    #[serde(rename = "choice")]
    Choice { choices: Vec<Choice> },
}

#[derive(Serialize, Deserialize, Type)]
pub struct CreateExperiment {
    #[serde(flatten)]
    pub experiment_type: CreateExperimentType,
    pub name: String,
    pub presets: Vec<String>,
}

#[derive(Debug, Clone, Copy, Serialize, Deserialize, Type, PartialEq, Eq)]
pub enum CurrentPreset {
    A,
    B,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct SharedExperiment {
    pub name: String,
    pub presets: HashMap<String, Preset>,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct ChoiceExperiment {
    #[serde(flatten)]
    pub shared: SharedExperiment,

    pub choices: Vec<Choice>,
}

impl ChoiceExperiment {
    pub fn new(name: String, presets: HashMap<String, Preset>, choices: Vec<Choice>) -> Self {
        Self {
            shared: SharedExperiment { name, presets },
            choices,
        }
    }
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct RatingExperiment {
    #[serde(flatten)]
    pub shared: SharedExperiment,

    pub order: Vec<String>,
}

impl RatingExperiment {
    pub fn new(name: String, presets: HashMap<String, Preset>, order: Vec<String>) -> Self {
        Self {
            shared: SharedExperiment { name, presets },
            order,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, Type, EnumTryAs)]
#[serde(tag = "experiment_type")]
pub enum Experiment {
    #[serde(rename = "rating")]
    Rating(RatingExperiment),
    #[serde(rename = "choice")]
    Choice(ChoiceExperiment),
}
