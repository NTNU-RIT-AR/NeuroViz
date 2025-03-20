// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
mod api;
mod consts;
mod structs;

use api::{commands, tcpservice};

use crate::structs::RenderParams;

use tauri::Manager;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::set_param,
            commands::get_param,
            commands::get_ip_address,
            commands::list_files,
            commands::retrieve_preset,
            commands::list_presets,
            commands::save_preset
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
