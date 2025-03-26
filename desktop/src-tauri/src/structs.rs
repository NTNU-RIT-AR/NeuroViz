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

#[derive(Deserialize, Serialize, Clone)]
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
    Rating,

    /// Choose between two options
    #[serde(rename = "choice")]
    Choice { choices: Vec<Choice> },
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
    pub presets: HashMap<String, Preset>,
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

impl ExperimentResult {
    //TODO lage funksjoner for å starte opp og å avslutte eksperimentet. Når man avslutter så lagres det til fil i korresponderende folder
    pub fn new(experiment: Experiment, observer_id: u64, note: String) -> Self {
        Self {
            experiment_type: match experiment.experiment_type {
                ExperimentType::Choice {..} => ExperimentResultType::Choice { choices: Vec::new() },
                ExperimentType::Rating => ExperimentResultType::Rating { ratings: Vec::new() }
            },
            name: experiment.name,
            observer_id,
            time: Local::now(),
            note,
            presets: experiment.presets
        }
    }

    pub fn save() {
        //TODO lagre ExperimentResult til fil
    }
}
