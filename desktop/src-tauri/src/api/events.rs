use std::path::PathBuf;

use serde::{Deserialize, Serialize};
use specta::Type;
use tauri_specta::Event;

use crate::state::AppState;

#[derive(Serialize, Deserialize, Debug, Clone, Type, Event)]
pub struct ConnectionEvent {
    pub is_connected: bool,
}

#[derive(Serialize, Deserialize, Debug, Clone, Type, Event)]
pub struct StateEvent {
    pub state: AppState,
}

#[derive(Serialize, Deserialize, Debug, Clone, Type, Event)]
pub struct ResultSavedEvent {
    pub result_file_path: PathBuf,
}
