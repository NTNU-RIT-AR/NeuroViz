pub mod storage {
    use std::fs::File;
    use std::io::Write;
    use std::path;
    use std::{fs, io};
    use serde_json::json;
    use slug::slugify;

    use crate::consts::Folder;
    use crate::structs::Preset;

    pub fn get_preset_from_preset_name(preset_name: String) -> Result<Preset, String> {

        let preset_file_content = match read_from_json_file(Folder::Presets, format!("{}.json", slugify(preset_name))) {
            Ok(content) => content,
            Err(e) => return Err(e)
        };
        
        let preset : Preset = match serde_json::from_str(&preset_file_content){
            Ok(p) => p,
            Err(e) => return Err(String::from(format!("Could not parse preset json files: {}", e.to_string()))),
        };
        
        Ok(preset)  
    }

    pub fn get_data_dir() -> Option<path::PathBuf> {
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

    pub fn make_file_list(path: path::PathBuf) -> io::Result<Vec<String>> {
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

    pub fn create_and_write_to_json_file(
        contents: String,
        folder: Folder,
        filename: String,
    ) -> Result<(), String> {
        let mut path = get_data_dir().unwrap();
        path.push(folder.as_str());
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

    pub fn read_from_json_file(folder: Folder, filename: String) -> Result<String, String> {
        let path = match get_data_dir() {
            Some(mut path) => {
                path.push(folder.as_str());
                path.push(filename);
                path
            }
            None => return Err(String::from("Could not get data dir path")),
        };
        let contents = match fs::read_to_string(path) {
            Ok(content) => content,
            Err(err) => {
                return Err(format!(
                    "Could not read from file with given path {}",
                    err.to_string()
                ))
            }
        };
        Ok(contents)
    }

    pub fn list_files(folder: Folder) -> Result<Vec<String>, String> {
        let path = match get_data_dir() {
            Some(mut path) => {
                path.push(folder.as_str());
                path
            }
            None => return Err(format!("could not get data dir path")),
        };

        match make_file_list(path) {
            Ok(files) => Ok(files),
            Err(e) => Err(format!("could not generate file list ({}): {}", folder, e)),
        }
    }
}