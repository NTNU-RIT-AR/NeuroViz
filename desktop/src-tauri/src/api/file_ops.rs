use dirs;
use std::{
    fs::{self, File},
    io::{self, ErrorKind, Write},
    path::PathBuf,
};

pub fn get_data_dir() -> Option<PathBuf> {
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

pub fn make_file_list(path: PathBuf) -> io::Result<Vec<String>> {
    let mut list: Vec<String> = vec![];
    if path.is_dir() {
        for entry in fs::read_dir(path)? {
            let file_name = entry?
                .file_name()
                .into_string()
                .map_err(|_| ErrorKind::InvalidData)?;
            list.push(file_name);
        }
    }
    Ok(list)
}

pub fn read_from_json_file(folder_path: &str, filename: String) -> Result<String, String> {
    let path = match get_data_dir() {
        Some(mut path) => {
            path.push(folder_path);
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

pub fn create_and_write_to_json_file(
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

//fn delete_file(file_name: String) -> Result<(), String> {
//    let folder_path = fs::read_to_string(file_name).map_err(|e| e.to_string())?;
//    let file_path = BaseDirectory::AppData + "/" + &file_name;
//
//    println!("Document path: {}", folder_path);
//
//    //fs::remove_file(file_path).map_err(|e| e.to_string())?;
//    Ok(())
//}
