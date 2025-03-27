// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
pub mod api;
pub mod consts;
pub mod structs;
pub mod appdata;

use appdata::AppData;
use api::{commands, http_server};

use structs::ExperimentPrompt;
use structs::UnityExperimentType;
use tauri::Manager;
use tokio::sync::{watch, mpsc};


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
            commands::create_preset,
            commands::list_experiments,
            commands::retrieve_experiment,
            commands::create_experiment,
            commands::start_experiment
        ])
        .setup(|app| {
            let (watch_sender, watch_receiver) = watch::channel(http_server::UnityState::Idle);
            let (signal_sender, mut signal_reciever) = mpsc::channel(10);

            let http_server = http_server::HttpServer {
                state: watch_receiver,
                sender: signal_sender
            };

            app.manage(AppData::new(watch_sender));

            //Starts the HTTP server to listen for incoming traffic
            tauri::async_runtime::spawn(http_server.run());

            let app_handle = app.app_handle().clone();

            //Listen from signals to swap preset
            tauri::async_runtime::spawn(async move {
                while let Some(_) = signal_reciever.recv().await {
                    println!("Got signal to swap!");
                    let app_data = app_handle.state::<AppData>();
                    if let Ok(preset) = app_data.swap_current_preset()  {
                        app_data.watch_sender.send(http_server::UnityState::Experiment { prompt: ExperimentPrompt {experiment_type: UnityExperimentType::Choice, preset} }).unwrap();
                    };
                }
            });       
            
            println!("{:?}", commands::start_experiment(app.handle().clone(), String::from("example-experiment-1"), 0, String::from("my note hihi")) );

            
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
