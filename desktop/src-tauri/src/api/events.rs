use tauri::{AppHandle, Emitter};

use crate::appdata::AppState;

pub fn emit_connection_event(app_handle: &AppHandle) {
    app_handle
        .emit("connection", "A device has connected")
        .unwrap();
}

pub fn emit_disconnection_event(app_handle: AppHandle) {
    app_handle
        .emit("disconnection", "A device has disconnected")
        .unwrap();
}

pub fn emit_app_state(app_handle: &AppHandle, app_state: AppState) {
    app_handle.emit("state", app_state).unwrap();
}
