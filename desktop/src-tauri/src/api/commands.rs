pub mod commands {
    use std::fs::File;
    use std::io::Write;
    use std::path;
    use std::{fs, io};

    use crate::structs::RenderParams;

    use super::super::super::consts::FOLDER_PRESETS;
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
    pub fn update_slider(app: tauri::AppHandle, parameter_name: &str, value: f32) {
        let state = app.state::<RenderParams>();
        let mut params = state.lock().unwrap();
        match parameter_name {
            "Hue" => params.hue = value,
            "Smoothness" => params.smoothness = value,
            "Metallic" => params.metallic = value,
            "Emission" => params.emission = value,
            _ => {}
        }
        println!("Updated slider values {:?}", params);
    }

    fn get_data_dir() -> Option<path::PathBuf> {
        //let mut path = env::current_exe().ok()?;
        //path.push("data");

        // Linux    ~/.local/share/xreal_control
        // macOS    ~/Library/Application Support\xreal_control
        // Windows  C:\Users\Alice\AppData\Roaming\xreal_control
        let mut path = dirs::data_dir()?;
        path.push("xreal_control");

        println!("data dir: {}", path.display());
        Some(path)
    }

    fn make_file_list(path: path::PathBuf) -> io::Result<Vec<String>> {
        let mut list: Vec<String> = vec![];
        if path.is_dir() {
            for entry in fs::read_dir(path)? {
                let file_name = entry?
                    .file_name()
                    .into_string()
                    .map_err(|_| io::ErrorKind::InvalidData)?;
                list.push(file_name);
            }
        }
        Ok(list)
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

    //fn delete_file(file_name: String) -> Result<(), String> {
    //    let folder_path = fs::read_to_string(file_name).map_err(|e| e.to_string())?;
    //    let file_path = BaseDirectory::AppData + "/" + &file_name;
    //
    //    println!("Document path: {}", folder_path);
    //
    //    //fs::remove_file(file_path).map_err(|e| e.to_string())?;
    //    Ok(())
    //}

    #[tauri::command]
    pub fn save_preset(app: tauri::AppHandle, preset_name: String) -> Result<(), String> {
        // Parse PARAMS to JSON
        let state = app.state::<RenderParams>();
        let params = state.lock().unwrap().clone();
        let json_string = serde_json::to_string(&params).unwrap();

        create_and_write_to_json_file(json_string, FOLDER_PRESETS.to_string(), preset_name)
    }

    fn create_and_write_to_json_file(
        contents: String,
        folder_path: String,
        filename: String,
    ) -> Result<(), String> {
        let mut path = get_data_dir().unwrap();
        path.push(folder_path);
        path.push(format!("{}.json", filename));

        // TODO: Se om vi kan fikse bedre error handling
        let mut file = File::create_new(path).map_err(|err| {
            format!(
                "Preset with this name exist already (?)\n {}",
                err.to_string()
            )
        })?;

        // {
        //     Ok(f) => f,
        //     Err(err ) => match err.kind() {
        //         Errorkind::AlreadyExists => return Err(String::from("Preset with this name already exists.")),
        //         _ => return return Err(String::from("Unknown error.")),
        //     }
        // };

        file.write_all(contents.as_bytes())
            .map_err(|err| format!("Could not write to file\n {}", err.to_string()))
    }
}
