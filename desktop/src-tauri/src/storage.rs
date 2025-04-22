use std::path::PathBuf;

use anyhow::{bail, Context};
use dirs;
use serde::de::DeserializeOwned;
use serde::Serialize;
use tokio::fs::{self, File};
use tokio::io::AsyncWriteExt;

use super::commands::WithKey;

#[derive(Debug)]
pub enum Folder {
    Presets,
    Experiments,
    Results { experiment_key: String },
}

impl Folder {
    pub fn path(&self) -> String {
        match self {
            Folder::Presets => "presets".to_owned(),
            Folder::Experiments => "experiments".to_owned(),
            Folder::Results {
                experiment_key: experiment,
            } => format!("results/{experiment}"),
        }
        .to_string()
    }
}

pub fn data_folder() -> anyhow::Result<PathBuf> {
    let path = if cfg!(debug_assertions) {
        // debug mode
        let manifest_dir = std::env::var("CARGO_MANIFEST_DIR").expect("CARGO_MANIFEST_DIR not set");
        let mut desktop_dir = PathBuf::from(manifest_dir);

        desktop_dir.pop();
        desktop_dir.join("data")
    } else {
        // release mode
        dirs::data_dir()
            .context("Could not get data dir")?
            .join("NeuroViz")
    };

    Ok(path)
}

pub async fn get_folder(folder: Folder) -> anyhow::Result<PathBuf> {
    let mut path = data_folder()?;

    path.push(folder.path());

    fs::create_dir_all(&path)
        .await
        .context("Could not create directory")?;

    println!("data dir: {}", path.display());
    Ok(path)
}

pub async fn create_file(
    key: &str,
    contents: impl Serialize,
    folder: Folder,
) -> anyhow::Result<PathBuf> {
    let file_name = format!("{}.json", key);
    let path = get_folder(folder).await?.join(file_name);

    let json = serde_json::to_string_pretty(&contents).context("Could not serialize to JSON")?;

    let mut file = File::create_new(&path)
        .await
        .context("File with this name exist already")?;

    file.write_all(json.as_bytes())
        .await
        .context("Could not write to file")?;

    Ok(path)
}

pub async fn read_file<T: DeserializeOwned>(key: &str, folder: Folder) -> anyhow::Result<T> {
    let file_name = format!("{}.json", key);
    let path = get_folder(folder).await?.join(file_name);

    let file_content = fs::read_to_string(&path)
        .await
        .context("Could not read file")?;

    let deserialized =
        serde_json::from_str::<T>(&file_content).context("Could not deserialize JSON")?;

    Ok(deserialized)
}

pub async fn read_files<T: DeserializeOwned>(folder: Folder) -> anyhow::Result<Vec<WithKey<T>>> {
    let path = get_folder(folder).await.context("Could not open folder")?;

    if !path.is_dir() {
        bail!("Folder does not exist");
    }

    let mut dir = fs::read_dir(path)
        .await
        .context("Could not read directory")?;

    let mut results = vec![];

    while let Some(entry) = dir
        .next_entry()
        .await
        .context("Failed to read directory entry")?
    {
        let file_content = fs::read_to_string(entry.path()).await?;

        let deserialized = serde_json::from_str::<T>(&file_content)
            .context("Failed to deserialize JSON content")?;

        let path = entry.path();

        let file_without_extension = path
            .file_stem()
            .context("Could not get file stem from path")?;

        let file_without_extension = file_without_extension
            .to_str()
            .context("Could not convert OsStr to str")?;

        results.push(WithKey {
            key: file_without_extension.to_owned(),
            value: deserialized,
        });
    }

    Ok(results)
}

pub async fn delete_file(key: &str, folder: Folder) -> anyhow::Result<()> {
    let file_name = format!("{}.json", key);
    let path = get_folder(folder).await?.join(file_name);

    fs::remove_file(&path)
        .await
        .context("Could not delete file")?;

    Ok(())
}
