use std::path::PathBuf;

use dirs;
use serde::de::DeserializeOwned;
use serde::Serialize;
use tokio::fs::{self, File};
use tokio::io::AsyncWriteExt;

use super::commands::WithKey;
use crate::consts::Folder;

pub async fn get_folder(folder: Folder) -> Result<PathBuf, String> {
    //let mut path = env::current_exe().ok()?;
    //path.push("data");

    let mut path;

    if cfg!(debug_assertions) {
        // debug mode}
        let manifest_dir = std::env::var("CARGO_MANIFEST_DIR").expect("CARGO_MANIFEST_DIR not set");
        let mut desktop_dir = PathBuf::from(manifest_dir);

        desktop_dir.pop();
        path = desktop_dir.join("data");
    } else {
        // release mode
        path = dirs::executable_dir().ok_or_else(|| format!("Could not get executable dir"))?;
    }
    path.push(folder.to_string());

    fs::create_dir_all(&path)
        .await
        .map_err(|e| format!("Could not create directory: {}", e))?;

    println!("data dir: {}", path.display());
    Ok(path)
}

pub async fn create_file(
    key: String,
    contents: impl Serialize,
    folder: Folder,
) -> Result<(), String> {
    let file_name = format!("{}.json", key);
    let path = get_folder(folder).await?.join(file_name);

    let json = serde_json::to_string_pretty(&contents)
        .map_err(|err| format!("Could not serialize to JSON: {}", err.to_string()))?;

    let mut file = File::create_new(path).await.map_err(|err| {
        format!(
            "File with this name exist already (?)\n {}",
            err.to_string()
        )
    })?;

    file.write_all(json.as_bytes())
        .await
        .map_err(|err| format!("Could not write to file\n {}", err.to_string()))
}

pub async fn read_file<T: DeserializeOwned>(key: String, folder: Folder) -> Result<T, String> {
    let file_name = format!("{}.json", key);
    let path = get_folder(folder).await?.join(file_name);

    let file_content = fs::read_to_string(&path)
        .await
        .map_err(|e| format!("Could not read file: {}", e))?;

    let deserialized = serde_json::from_str::<T>(&file_content).map_err(|e| e.to_string())?;

    Ok(deserialized)
}

pub async fn read_files<T: DeserializeOwned>(folder: Folder) -> Result<Vec<WithKey<T>>, String> {
    let path = get_folder(folder)
        .await
        .map_err(|e| format!("Could not open folder: {}", e))?;

    if !path.is_dir() {
        return Err(format!("Folder does not exist"));
    }

    let mut dir = fs::read_dir(path)
        .await
        .map_err(|_| "Couldnt read directory".to_string())?;

    let mut results = vec![];

    while let Some(entry) = dir.next_entry().await.map_err(|e| e.to_string())? {
        let file_content = fs::read_to_string(entry.path())
            .await
            .map_err(|e| e.to_string())?;

        let deserialized = serde_json::from_str::<T>(&file_content)
            .map_err(|e| format!("Could not deserialize JSON: {}", e))?;

        let path = entry.path();

        let file_without_extension = path
            .file_stem()
            .ok_or_else(|| format!("Could not get file stem from path: {:?}", entry.path()))?;

        let file_without_extension = file_without_extension
            .to_str()
            .ok_or_else(|| format!("Could not convert OsStr to str"))?;

        results.push(WithKey {
            key: file_without_extension.to_owned(),
            value: deserialized,
        });
    }

    Ok(results)
}

pub async fn delete_file(key: String, folder: Folder) -> Result<(), String> {
    let file_name = format!("{}.json", key);
    let path = get_folder(folder).await?.join(file_name);

    fs::remove_file(&path).await.map_err(|e| {
        format!(
            "Could not delete file '{}': {}",
            path.display(),
            e.to_string()
        )
    })?;

    Ok(())
}
