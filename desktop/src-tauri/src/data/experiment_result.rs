use std::collections::HashMap;

use chrono::{DateTime, Local};
use serde::{Deserialize, Serialize};
use specta::Type;
use strum::EnumTryAs;

use super::{
    experiment::{ChoiceExperiment, RatingExperiment},
    preset::Preset,
};

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct OutcomeChoice {
    pub a: String,
    pub b: String,
    pub selected: String,
    pub time: DateTime<Local>,
    pub duration: f64,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct OutcomeRating {
    pub preset: String,
    pub rank: u8,
    pub time: DateTime<Local>,
    pub duration: f64,
}

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub struct SharedExperimentResult {
    pub name: String,
    pub time: DateTime<Local>,
    pub observer_id: u32,
    pub note: String,
    pub presets: HashMap<String, Preset>,
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct ChoiceExperimentResult {
    #[serde(flatten)]
    pub shared: SharedExperimentResult,

    pub choices: Vec<OutcomeChoice>,
}

impl ChoiceExperimentResult {
    pub fn new(
        name: String,
        time: DateTime<Local>,
        observer_id: u32,
        note: String,
        experiment: &ChoiceExperiment,
    ) -> Self {
        Self {
            shared: SharedExperimentResult {
                name,
                time,
                observer_id,
                note,
                presets: experiment.shared.presets.clone(),
            },
            choices: Vec::new(),
        }
    }
}

#[derive(Debug, Serialize, Deserialize, Type, Clone)]
pub struct RatingExperimentResult {
    #[serde(flatten)]
    pub shared: SharedExperimentResult,

    pub ratings: Vec<OutcomeRating>,
}

impl RatingExperimentResult {
    pub fn new(
        name: String,
        time: DateTime<Local>,
        observer_id: u32,
        note: String,
        experiment: &RatingExperiment,
    ) -> Self {
        Self {
            shared: SharedExperimentResult {
                name,
                time,
                observer_id,
                note,
                presets: experiment.shared.presets.clone(),
            },
            ratings: Vec::new(),
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, Type, EnumTryAs)]
#[serde(tag = "experiment_type")]
pub enum ExperimentResult {
    #[serde(rename = "rating")]
    Rating(RatingExperimentResult),
    #[serde(rename = "choice")]
    Choice(ChoiceExperimentResult),
}
