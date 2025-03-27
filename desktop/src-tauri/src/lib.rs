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
            let (swap_signal_sender, mut signal_reciever) = mpsc::channel(10);
            let (answer_sender, mut answer_reciever) = mpsc::channel(10);

            let http_server = http_server::HttpServer {
                state: watch_receiver,
                swap_signal_sender,
                answer_sender
            };

            let app_handle = app.app_handle().clone();

            app.manage(AppData::new(watch_sender, app_handle.clone()));

            //Starts the HTTP server to listen for incoming traffic
            tauri::async_runtime::spawn(http_server.run());


            //Listen from signals to swap preset
            tauri::async_runtime::spawn(async move {
                while let Some(_) = signal_reciever.recv().await {
                    println!("Got signal to swap!");
                    let app_data = app_handle.state::<AppData>();
                    if let Err(e) = app_data.swap_current_preset()  {
                        println!("Could not swap preset: {}", e.to_string())
                    };
                }
            }); 

            let app_handle = app.app_handle().clone();

            //Listen for answers from experiment
            tauri::async_runtime::spawn(async move {
                while let Some(answer) = answer_reciever.recv().await {
                    println!("Got an answer from the experiment! {:?}", answer);
                }
            });
            
            println!("{:?}", commands::start_experiment(app.handle().clone(), String::from("example-experiment-1"), 0, String::from("my note hihi")) );

            
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
