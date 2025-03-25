// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
pub mod api;
pub mod consts;
pub mod structs;
pub mod appdata;

use appdata::AppData;
use api::{commands, http_server};

use tauri::Manager;
use tokio::sync::watch;


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
