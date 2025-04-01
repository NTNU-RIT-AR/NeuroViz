use serde::{Deserialize, Serialize};
use specta::Type;
use tauri_specta::Event;

use crate::appdata::AppState;

#[derive(Serialize, Deserialize, Debug, Clone, Type, Event)]
pub struct ConnectionEvent {
    pub is_connected: bool,
}

#[derive(Serialize, Deserialize, Debug, Clone, Type, Event)]
pub struct StateEvent {
    pub state: AppState,
}
