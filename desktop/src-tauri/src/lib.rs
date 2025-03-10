// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
use serde::Serialize;
use serde_json;
use tokio::{io::AsyncWriteExt, net::TcpListener};

use lazy_static::lazy_static;
use serde::Deserialize;
use std::{
    sync::{Arc, Mutex},
    time::Duration,
};
use tauri::{path::BaseDirectory, Manager};

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::update_slider,
            commands::get_ip_address
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
    static ref PARAMS: Arc<Mutex<Parameters>> = Arc::new(Mutex::new(Parameters {
        slider1: 1.0,
        slider2: 1.0,
        slider3: 1.0,
        slider4: 1.0,
    }));
}
