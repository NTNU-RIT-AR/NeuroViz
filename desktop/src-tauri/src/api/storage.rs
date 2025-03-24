pub mod storage {
    use std::fs::File;
    use std::io::Write;
    use std::path;
    use std::{fs, io};

    use crate::consts::Folder;

    use serde::de::DeserializeOwned;

    pub fn parse_from_json_file<T>(slugged_name: String, folder: Folder) -> Result<T, String>
    where
        T: DeserializeOwned,
    {

        let file_content = match read_from_json_file(folder, format!("{}.json", slugged_name)) {
            Ok(content) => content,
            Err(e) => return Err(e),
        };

        let parsed: T = match serde_json::from_str(&file_content) {
            Ok(p) => p,
            Err(e) => return Err(format!("Could not parse JSON file: {}", e)),
        };

        Ok(parsed)
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
                let entry = entry?;
                if let Some(file_stem) = entry.path().file_stem().and_then(|s| s.to_str()) {
                    list.push(file_stem.to_string());
                }
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
                "File with this name exist already (?)\n {}",
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
        let contents = match fs::read_to_string(&path) {
            Ok(content) => content,
            Err(err) => {
                return Err(format!(
                    "Could not read from file with given path {:?} {}",
                    path,
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