use crate::structs::ExperimentResult;
use crate::structs::Experiment;
use crate::structs::RenderParams;
use crate::structs::RenderParamsInner;
use crate::http_server::UnityState;


use crate::api::http_server;

use std::sync::Arc;
use std::sync::Mutex;
use tokio::sync::watch;

#[derive(Clone)]
pub enum AppState {
    ExperimentMode {experiment_result: ExperimentResult, experiment: Experiment},
    LiveViewMode
}

#[derive(Clone)]
pub struct AppData {
    //TODO flytte params inn i LiveViewMode
    pub params: Arc<Mutex<RenderParamsInner>>,
    pub watch_sender: watch::Sender<UnityState>,
    pub app_state: AppState
}

impl AppData {
    pub fn new(watch_sender: watch::Sender<UnityState>) -> Self {
        AppData { params: Arc::new(RenderParams::default()), watch_sender: watch_sender, app_state: AppState::LiveViewMode }
    }

    pub fn set_params(&self, param_name: &str, value: f64){
        let value = value as f32;
        let mut params = self.params.lock().unwrap();
        println!("Updated slider values {:?}", params);
        match param_name {
            "Hue" => params.hue = value,
            "Smoothness" => params.smoothness = value,
            "Metallic" => params.metallic = value,
            "Emission" => params.emission = value,
            _ => {}
        }

        self.watch_sender.send(http_server::UnityState::Live { parameters: params.clone() }).unwrap();
    }

    pub fn get_param(&self, param_name: &str) -> f64 {
        let params = self.params.lock().unwrap();
        println!("Returned param:  {:?}", params);
        return match param_name {
            "Hue" => params.hue,
            "Smoothness" => params.smoothness,
            "Metallic" => params.metallic,
            "Emission" => params.emission,
            _ => panic!("param mismatch"),
        } as f64;
    }

    pub fn get_state(&self) -> &AppState {
        &self.app_state
    }
}