pub mod events {
    use tauri::{Emitter, AppHandle};

    pub fn emit_connection_event(app_handle: &AppHandle) {
        app_handle.emit("connection", "A device has connected").unwrap();
    }
    
    pub fn emit_disconnection_event(app_handle: AppHandle) {
        app_handle.emit("disconnection", "A device has disconnected").unwrap();
    }
}