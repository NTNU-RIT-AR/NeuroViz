// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
pub mod api;
pub mod appdata;
pub mod consts;
pub mod structs;

use api::http_server::{HttpServer, UnityState};
use api::{commands, http_server};
use appdata::{AppData, AppState};

use futures_signals::signal::SignalExt;
use structs::UnityExperimentType;
use structs::{ExperimentPrompt, ExperimentType};
use tauri::{Emitter, Manager};
use tokio::sync::mpsc;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::set_param,
            commands::get_param,
            commands::get_ip_address,
            commands::list_presets,
            commands::retrieve_preset,
            commands::list_experiments,
            commands::retrieve_experiment,
            commands::create_experiment,
            commands::start_experiment
        ])
        .setup(|app| {
            let app_handle = app.handle().clone();
            let app_data = AppData::new();
            app.manage(app_data.clone());

            tauri::async_runtime::spawn({
                let app_data = app_data.clone();

                async move {
                    app_data
                        .state
                        .signal_cloned()
                        .for_each(async |state| {
                            println!("App state changed");
                            app_handle.emit("state", state).unwrap();
                        })
                        .await;
                }
            });

            // Initialize the HTTP server
            let (unity_event_sender, mut unity_event_receiver) = mpsc::unbounded_channel();
            let http_server = HttpServer::new(
                app_data.state.get_cloned().into(),
                app_data.state.signal_cloned().map(|state| state.into()),
                unity_event_sender,
            );

            // Starts the HTTP server
            tauri::async_runtime::spawn(http_server.run());

            // Listen to events from HTTP server
            tauri::async_runtime::spawn(async move {
                while let Some(event) = unity_event_receiver.recv().await {
                    match event {
                        http_server::UnityEvent::SwapPreset => {
                            println!("Got signal to swap!");
                            let mut app_state = app_data.state.lock_mut();

                            let Some(experiment_state) = app_state.try_as_experiment_mut() else {
                                // Not in experiment mode, ignore
                                continue;
                            };

                            experiment_state.swap_current_preset();
                        }
                        http_server::UnityEvent::Answer(experiment_answer) => todo!(),
                    }
                }
            });

            // println!(
            //     "{:?}",
            //     commands::start_experiment(
            //         app.handle().clone(),
            //         String::from("example-experiment-1"),
            //         0,
            //         String::from("my note hihi")
            //     )
            // );

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
