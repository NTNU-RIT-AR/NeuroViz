use tauri_neuroviz_lib::{generate_typescript_types, tauri_commands};

pub fn main() {
    let builder = tauri_commands();

    generate_typescript_types(&builder);
}
