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

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::set_param,
            commands::get_param,
            commands::get_ip_address,
            commands::list_presets,

            commands::get_preset,
            commands::create_preset,

            commands::get_experiment,
            commands::create_experiment

            commands::list_experiments,
            commands::get_all_experiments,
            commands::start_experiment
        ])
        .setup(|app| {
            let (watch_sender, watch_receiver) = watch::channel(http_server::UnityState::Idle);

            let http_server = http_server::HttpServer {
                state: watch_receiver,
            };

            app.manage(AppData::new(watch_sender));

            tauri::async_runtime::spawn(http_server.run());

            // println!("{:?}", commands::start_experiment(app.handle().clone(), String::from("example-experiment-1"), 0, String::from("my note hihi")) );
            
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
