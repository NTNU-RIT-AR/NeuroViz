use tauri::{Emitter, AppHandle};

use crate::structs::Preset;

pub fn emit_connection_event(app_handle: &AppHandle) {
    app_handle.emit("connection", "A device has connected").unwrap();
}

pub fn emit_disconnection_event(app_handle: AppHandle) {
    app_handle.emit("disconnection", "A device has disconnected").unwrap();
}

pub fn emit_swap_preset_in_experiment(app_handle: &AppHandle, payload: &Preset) {
    app_handle.emit("swap", payload).unwrap();
}