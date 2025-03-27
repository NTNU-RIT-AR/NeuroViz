use std::collections::HashMap;
use std::sync::Mutex;

use chrono::{DateTime, Local};
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Clone, Debug, Default, PartialEq)]
pub struct RenderParamsInner {
    pub hue: f32,
    pub smoothness: f32,
    pub metallic: f32,
    pub emission: f32,
}

pub type RenderParams = Mutex<RenderParamsInner>;

//TODO: implement something like this for easier access to renderparams
//pub fn aquire_render_params<'a>(app: &'a AppHandle) -> MutexGuard<'a, RenderParamsInner> {
//    let state = app.state::<RenderParams>();
//    state.lock().unwrap()
//}

#[derive(Deserialize, Serialize, Clone)]
pub struct Choice {
    pub a: String,
    pub b: String,
}

#[derive(Deserialize, Debug)]
pub struct Answer {
    #[serde(flatten)]
    pub experiment_type: AnswerExperimentType
}

#[derive(Deserialize, Debug)]
#[serde(tag = "experiment_type")]
pub enum AnswerExperimentType {
    #[serde(rename="rating")]
    Rating {value: u8},
    #[serde(rename="choice")]
    Choice
}

#[derive(Deserialize, Serialize, Clone, Debug, PartialEq)]
pub struct Preset {
    pub name: String,
    pub parameters: RenderParamsInner,
}

#[derive(Deserialize, Serialize, Clone)]
pub struct OutcomeChoice {
    pub a: String,
    pub b: String,
    pub selected: String,
    pub time: DateTime<Local>,
    pub duration_on_a: f64,
    pub duration_on_b: f64,
    pub duration: f64
}

#[derive(Deserialize, Serialize, Clone)]
pub struct OutcomeRating {
    pub preset: String,
    pub rank: u8,
    pub time: DateTime<Local>,
    pub duration: f64
}

#[derive(Serialize, Deserialize, Clone)]
#[serde(tag = "experiment_type")]
pub enum ExperimentType {
    /// Rating between 1-5
    #[serde(rename = "rating")]
    Rating {order: Vec<String>},

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
    pub preset: Preset,
}

#[derive(Deserialize, Serialize, Clone)]
#[serde(tag = "experiment_type")]
pub enum ExperimentResultType {
    #[serde(rename = "rating")]
    Rating {ratings: Vec<OutcomeRating>},
    #[serde(rename = "choice")]
    Choice {choices: Vec<OutcomeChoice>}
}

#[derive(Serialize, Deserialize, Clone)]
pub struct Experiment {
    #[serde(flatten)]
    pub experiment_type: ExperimentType,
    pub name: String,
    pub presets: HashMap<String, Preset>
}

#[derive(Deserialize)]
pub struct CreateExperiment {
    #[serde(flatten)]
    pub experiment_type: ExperimentType,
    pub name: String,
    pub presets: Vec<String>,
}

#[derive(Deserialize, Serialize, Clone)]
pub struct ExperimentResult {
    #[serde(flatten)]
    pub experiment_type: ExperimentResultType,
    pub name: String,
    pub time: DateTime<Local>,
    pub observer_id: u64,
    pub note: String,
    pub presets: HashMap<String, Preset>
}

#[derive(Clone)]
pub enum CurrentPreset {
    A,
    B
}

#[derive(Clone)]
pub struct ExperimentState {
    pub current_index: usize,
    pub choice_current_preset: CurrentPreset
}

impl ExperimentState {
    pub fn new() -> Self {
        Self {
            current_index: 0,
            choice_current_preset: CurrentPreset::A
        }
    }
}

impl ExperimentResult {
    //TODO lage funksjon for å avslutte eksperimentet. Når man avslutter så lagres det til fil i korresponderende folder
    pub fn new(experiment: &Experiment, observer_id: u64, note: String) -> Self {
        Self {
            experiment_type: match experiment.experiment_type {
                ExperimentType::Choice {..} => ExperimentResultType::Choice { choices: Vec::new() },
                ExperimentType::Rating {..} => ExperimentResultType::Rating { ratings: Vec::new() }
            },
            name: experiment.name.clone(),
            observer_id,
            time: Local::now(),
            note,
            presets: experiment.presets.clone()
        }
    }

    pub fn save() {
        //TODO lagre ExperimentResult til fil
    }
}
