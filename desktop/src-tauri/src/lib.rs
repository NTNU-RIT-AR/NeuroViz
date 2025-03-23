// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
mod api;
mod consts;
mod structs;

use api::commands::commands;
use api::tcpservice::tcpservice;

use crate::structs::RenderParams;
use crate::structs::CreateExperiment;
use crate::structs::Choice;

use tauri::Manager;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {

    // println!("{:?}", commands::create_experiment(CreateExperiment {
    //     experiment_type: String::from("choice"),
    //     name: String::from("My test experiment"),
    //     presets: Vec::from([String::from("High emission"), String::from("Metal looking"), String::from("Very hue")]),
    //     choices: Vec::from([Choice {a: String::from("high-emission"), b: String::from("metal-looking")}, Choice {a: String::from("very-hue"), b: String::from("metal-looking")}])
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
            app.manage(RenderParams::default());

            let app_handle = app.app_handle().clone();
            tauri::async_runtime::spawn(tcpservice::tcp_listener(app_handle));
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
