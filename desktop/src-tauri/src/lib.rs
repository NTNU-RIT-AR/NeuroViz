// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
mod api;
mod structs;
mod consts;
use crate::structs::Parameters;
use api::commands::commands;
use api::tcpservice::tcpservice;

use tauri::{path::BaseDirectory, Manager};

use lazy_static::lazy_static;
use std::sync::{Arc, Mutex};

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::update_slider,
            commands::get_ip_address,
            commands::list_files,
            commands::save_preset
        ])
        .setup(|app| {
            // generate the data directory path and pass to manager
            let path = app.path().resolve("ar-renderer", BaseDirectory::Data)?;
            app.manage(path);

            let app_handle = app.app_handle().clone();
            tauri::async_runtime::spawn(tcpservice::tcp_listener(app_handle));
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

lazy_static! {
    pub static ref PARAMS: Arc<Mutex<Parameters>> = Arc::new(Mutex::new(Parameters {
        hue: 1.0,
        smoothness: 1.0,
        metallic: 1.0,
        emission: 1.0,
    }));
}
