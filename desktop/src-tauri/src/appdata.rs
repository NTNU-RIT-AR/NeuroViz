use crate::structs::{UnityExperimentType, RenderParamsInner, RenderParams, Preset, ExperimentType, Experiment, ExperimentState, ExperimentResult, ExperimentPrompt, CurrentPreset};
use crate::http_server::UnityState;


use crate::api::{events, http_server};

use std::sync::Arc;
use std::sync::Mutex;
use tauri::AppHandle;
use tauri::Emitter;
use tokio::sync::watch;

#[derive(Clone)]
pub enum AppState {
    ExperimentMode {experiment_result: ExperimentResult, experiment: Experiment, experiment_state: ExperimentState},
    LiveViewMode
}

#[derive(Clone)]
pub struct AppData {
    //TODO flytte params inn i LiveViewMode
    pub params: Arc<Mutex<RenderParamsInner>>,
    pub watch_sender: watch::Sender<UnityState>,
    pub app_state: Arc<Mutex<AppState>>,
    pub app_handle: AppHandle
}

impl AppData {
    pub fn new(watch_sender: watch::Sender<UnityState>, app_handle: AppHandle) -> Self {
        AppData { params: Arc::new(RenderParams::default()), watch_sender, app_state: Arc::new(Mutex::new(AppState::LiveViewMode)), app_handle }
    }

    pub fn set_param(&self, param_name: &str, value: f64){
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

    pub fn get_state(&self) -> AppState {
        let app_state = self.app_state.lock().unwrap();
        app_state.clone()
    }

    pub fn set_state(&self, new_state: AppState) {
        let mut app_state = self.app_state.lock().unwrap(); // Get mutable access
        (*app_state) = new_state;
    }

    pub fn get_current_preset(&self, swap: Option<()>) -> Result<Preset, String> {
        let mut app_state = self.app_state.lock().unwrap();
        let app_state = &mut *app_state;
        //Check if current state is AppState::ExperimentMode

        let AppState::ExperimentMode { experiment_result: _, experiment, experiment_state } = app_state else {
            return Err(String::from("Can not get current preset when in LiveViewMode"));
        };

        match &mut experiment.experiment_type {
            ExperimentType::Choice {choices} => return match (&mut experiment_state.choice_current_preset, swap) {
                (CurrentPreset::A, None) | (CurrentPreset::B, Some(_)) => {
                    if swap == Some(()) {
                        experiment_state.choice_current_preset = CurrentPreset::A;
                    }
                    Ok(experiment.presets[&choices[experiment_state.current_index].a].clone())
                },
                (CurrentPreset::B, None) | (CurrentPreset::A, Some(_)) => {
                    if swap == Some(()) {
                        experiment_state.choice_current_preset = CurrentPreset::B;
                    }
                    Ok(experiment.presets[&choices[experiment_state.current_index].b].clone())
                }
            },
            ExperimentType::Rating { order } => return Ok(experiment.presets[&order[experiment_state.current_index]].clone())
        };
    }

    pub fn swap_current_preset(&self) -> Result<Preset, String> {
        match self.get_current_preset(Some(())) {
            Ok(preset) => {
                events::emit_swap_preset_in_experiment(&self.app_handle, &preset);
                self.watch_sender.send(http_server::UnityState::Experiment { prompt: ExperimentPrompt {experiment_type: UnityExperimentType::Choice, preset: preset.clone()} }).unwrap();
                Ok(preset)
            },
            Err(e) => Err(e)
        }
    }
}