use min_tauri_app_lib::{generate_typescript_types, tauri_commands};

pub fn main() {
    let builder = tauri_commands();

    generate_typescript_types(&builder);
}
