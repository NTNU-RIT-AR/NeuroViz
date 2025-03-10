pub mod commands {
    use std::fs;
    use std::io;
    use std::path;

    use super::super::super::PARAMS;
    use local_ip_address::local_ip;

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
    pub fn update_slider(parameter_name: &str, value: f32) {
        let mut params = PARAMS.lock().unwrap();
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
        if let Some(mut path) = dirs::data_dir() {
            path.push("/xreal_control");
            Some(path)
        } else {
            None
        }

        // This only works given that we have the app handle, using dirs is simpler
        // get_data_dir(app: tauri::AppHandle)
        //match app.path().data_dir() {
        //    Ok(mut base_path) => {
        //        //TODO: Secure with pathbuf to stop traversal attacks
        //        base_path.push("/xreal_control");
        //        Ok(base_path)
        //    }
        //    Err(_) => Err("Could not get data dir".to_string()),
        //}
    }

    fn make_file_list(path: path::PathBuf) -> Result<Vec<String>, String> {
        //TODO: probably can simplify
        let files: Vec<String> = fs::read_dir(path)
            .map_err(|e| e.to_string())?
            .filter_map(|entry| {
                let mut opt: Option<String> = None;
                if let Ok(entry) = entry {
                    if entry.file_type().ok()?.is_file() {
                        // only files, not dirs
                        opt = entry.file_name().into_string().ok();
                    }
                }
                opt
            })
            .collect();

        Ok(files)
    }

    #[tauri::command]
    pub fn list_files(folder: String) -> Vec<String> {
        let path = match get_data_dir() {
            Some(mut path) => {
                path.push(folder);
                path
            }
            None => return vec![format!("could not get data dir path")],
        };

        println!("{}", path.display());

        match make_file_list(path) {
            Ok(file) => file,
            Err(e) => vec![format!("could not generate file list: {}", e)],
        }
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
}
