use std::fs::File;
use std::io::Write;
use std::path::{self, PathBuf};
use std::{fs, io};

use dirs;

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

pub fn get_folder(folder: Folder) -> Result<PathBuf, String> {
    //let mut path = env::current_exe().ok()?;
    //path.push("data");

    // Linux    ~/.local/share/xreal_control
    // macOS    ~/Library/Application Support\xreal_control
    // Windows  C:\Users\Alice\AppData\Roaming\xreal_control

    let mut path;

    if cfg!(debug_assertions) {
        // debug mode
        path = dirs::data_dir().ok_or_else(|| format!("Could not get data dir"))?;
        path.push("NeuroViz");
    } else {
        // release mode
        path = dirs::executable_dir().ok_or_else(|| format!("Could not get executable dir"))?;
    }
    path.push(folder.as_str());

    fs::create_dir_all(&path).map_err(|e| format!("Could not create directory: {}", e))?;

    println!("data dir: {}", path.display());
    Ok(path)
}

pub fn create_and_write_to_json_file(
    contents: String,
    folder: Folder,
    filename: String,
) -> Result<(), String> {
    let mut path = get_folder(folder).unwrap();
    path.push(format!("{}.json", filename));

    // TODO: Se om vi kan fikse bedre error handling
    let mut file = File::create_new(path).map_err(|err| {
        format!(
            "File with this name exist already (?)\n {}",
            err.to_string()
        )
    })?;

    file.write_all(contents.as_bytes())
        .map_err(|err| format!("Could not write to file\n {}", err.to_string()))
}

pub fn read_from_json_file(folder: Folder, filename: String) -> Result<String, String> {
    let mut path = get_folder(folder)?;
    path.push(filename);

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
    let path = get_folder(folder).map_err(|e| format!("Could not open folder: {}", e))?;
    let mut list: Vec<String> = vec![];

    if path.is_dir() {
        for entry in fs::read_dir(path).map_err(|_| "Couldnt read dir".to_string())? {
            let entry = entry.map_err(|e| e.to_string())?;
            if let Some(file_stem) = entry.path().file_stem().and_then(|s| s.to_str()) {
                list.push(file_stem.to_string());
            } else {
                return Err(format!("Could not get file stem"));
            }
        }
    } else {
        return Err(format!("Folder does not exist"));
    }
    Ok(list)
}
