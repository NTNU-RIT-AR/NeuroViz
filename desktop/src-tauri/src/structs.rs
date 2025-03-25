use std::collections::HashMap;
use std::sync::Mutex;

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

#[derive(Deserialize, Serialize)]
pub struct Choice {
    pub a: String,
    pub b: String,
}

#[derive(Deserialize, Serialize)]
pub struct Outcome {
    pub a: String,
    pub b: String,
    pub selected: String,
}

#[derive(Deserialize, Serialize)]
pub struct Preset {
    pub name: String,
    pub parameters: RenderParamsInner,
}

#[derive(Serialize, Deserialize)]
#[serde(tag = "experiment_type")]
pub enum ExperimentType {
    /// Rating between 1-5
    #[serde(rename = "rating")]
    Rating,

    /// Choose between two options
    #[serde(rename = "choice")]
    Choice { choices: Vec<Choice> },
}

#[derive(Serialize, Deserialize)]
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

#[derive(Deserialize, Serialize)]
pub struct Result {
    pub experiment_type: String,
    pub name: String,
    pub subject: String,
    pub note: String,
    pub presets: HashMap<String, Preset>,
    pub choices: Vec<Outcome>,
}

