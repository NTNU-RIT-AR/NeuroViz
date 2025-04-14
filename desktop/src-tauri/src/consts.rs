use core::fmt;
use std::path::Path;

use serde::{Deserialize, Serialize};
pub const HTTP_SERVER_PORT: u16 = 9001;

// pub const FOLDER_PRESETS: &str = "presets";
// pub const FOLDER_EXPERIMENTS: &str = "experiments";
// pub const FOLDER_RESULTS: &str = "results";

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
