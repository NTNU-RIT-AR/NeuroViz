use crate::api::file_ops::*;
use crate::consts;
use crate::structs::RenderParams;

use local_ip_address::local_ip;
use tauri::Manager;

#[tauri::command]
pub fn get_ip_address() -> String {
    match local_ip() {
        Ok(ip) => {
            println!("Local IP address: {}", ip);
            return format!("{}", ip);
        }
        Err(e) => {
            eprintln!("Error retrieving local IP: {}", e);
            return String::from("");
        }
    }
}

#[tauri::command]
pub fn set_param(app: tauri::AppHandle, param_name: &str, value: f64) {
    let state = app.state::<RenderParams>();
    let mut params = state.lock().unwrap();
    let value = value as f32;
    println!("Updated slider values {:?}", params);
    match param_name {
        "Hue" => params.hue = value,
        "Smoothness" => params.smoothness = value,
        "Metallic" => params.metallic = value,
        "Emission" => params.emission = value,
        _ => {}
    }
}
#[tauri::command]
pub fn get_param(app: tauri::AppHandle, param_name: &str) -> f64 {
    let state = app.state::<RenderParams>();
    let params = state.lock().unwrap();
    println!("Returned param:  {:?}", params);
    return match param_name {
        "Hue" => params.hue,
        "Smoothness" => params.smoothness,
        "Metallic" => params.metallic,
        "Emission" => params.emission,
        _ => panic!("param mismatch"),
    } as f64;
}

#[tauri::command]
pub fn list_files(folder: &str) -> Result<Vec<String>, String> {
    let path = match get_data_dir() {
        Some(mut path) => {
            path.push(folder);
            path
        }
        None => return Err(format!("could not get data dir path")),
    };

    match make_file_list(path) {
        Ok(files) => Ok(files),
        Err(e) => Err(format!("could not generate file list ({}): {}", folder, e)),
    }
}

#[tauri::command]
pub fn list_presets() -> Result<Vec<String>, String> {
    return list_files("presets");
}

#[tauri::command]
pub fn save_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
    // Parse PARAMS to JSON
    let state = app.state::<RenderParams>();
    let params = state.lock().unwrap().clone();
    let json_string = serde_json::to_string(&params).unwrap();

    create_and_write_to_json_file(json_string, consts::FOLDER_PRESETS.to_string(), preset_name)
}

#[tauri::command]
pub fn retrieve_preset(preset_name: String) -> Result<String, String> {
    read_from_json_file(consts::FOLDER_PRESETS, format!("{}.json", preset_name))
}
