// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
pub mod api;
pub mod consts;
pub mod structs;

use api::{commands, http_server};

use crate::structs::RenderParams;
use crate::structs::RenderParamsInner;
use crate::http_server::UnityState;

use std::sync::Arc;
use std::sync::Mutex;
use tauri::Manager;
use tokio::sync::watch;

#[derive(Clone)]
pub struct AppData {
    params: Arc<Mutex<RenderParamsInner>>,
    watch_sender: watch::Sender<UnityState>
}

impl AppData {
    fn new(watch_sender: watch::Sender<UnityState>) -> Self {
        AppData { params: Arc::new(RenderParams::default()), watch_sender: watch_sender }
    }

    fn set_params(&self, param_name: &str, value: f64){
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

    fn get_param(&self, param_name: &str) -> f64 {
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
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {

    // println!("{:?}", commands::create_experiment(CreateExperiment {
    //     experiment_type: ExperimentType::Choice { choices: Vec::from([Choice {a: String::from("high-emission"), b: String::from("metal-looking")}, Choice {a: String::from("very-hue"), b: String::from("metal-looking")}]) },
    //     name: String::from("My test experiment"),
    //     presets: Vec::from([String::from("High emission"), String::from("Metal looking"), String::from("Very hue")])
    // }));

    

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::set_param,
            commands::get_param,
            commands::get_ip_address,
            commands::list_presets,
            commands::retrieve_preset,
            commands::create_preset,
            commands::list_experiments,
            commands::retrieve_experiment,
            commands::create_experiment
        ])
        .setup(|app| {
            let (watch_sender, watch_receiver) = watch::channel(http_server::UnityState::Idle);

            let http_server = http_server::HttpServer {
                state: watch_receiver,
            };

            app.manage(AppData::new(watch_sender));

            tauri::async_runtime::spawn(http_server.run());
            
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
