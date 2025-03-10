pub mod commands {
    use serde::{Deserialize, Serialize};
    use std::{fs, path::Path};

    use super::super::super::PARAMS;
    use local_ip_address::local_ip;
    use tauri::{
        path::{BaseDirectory, PathResolver},
        Manager,
    };

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
    pub fn update_slider(slider_number: &str, slider_value: f32) {
        let mut params = PARAMS.lock().unwrap();
        match slider_number {
            "1" => params.slider1 = slider_value,
            "2" => params.slider2 = slider_value,
            "3" => params.slider3 = slider_value,
            "4" => params.slider4 = slider_value,
            _ => {}
        }
        println!("Updated slider values {:?}", params);
    }

    fn get_data_directory_path(app: tauri::AppHandle) -> String {
        // Accessing the AppHandle here
        //
        //let path: &std::path::Path = Manager::state::<Path>(&app);
        //path.to_str().unwrap_or("").to_string()
        String::new()
    }

    //#[tauri::command]
    pub fn list_files(folder: String) -> Result<Vec<String>, String> {
        //let path = PathResolver.data_dir();
        let path = "path";
        let path = format!("{}{}{}", "path", "/", &folder);
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
