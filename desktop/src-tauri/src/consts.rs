use core::fmt;

use serde::{Deserialize, Serialize};
pub const HTTP_SERVER_PORT: u16 = 9001;

// pub const FOLDER_PRESETS: &str = "presets";
// pub const FOLDER_EXPERIMENTS: &str = "experiments";
// pub const FOLDER_RESULTS: &str = "results";

#[derive(Debug, Serialize, Deserialize)]
pub enum Folder {
    #[serde(rename = "presets")]
    Presets,
    #[serde(rename = "experiments")]
    Experiments,
    #[serde(rename = "results")]
    Results,
}

impl fmt::Display for Folder {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{}", serde_json::to_string(&self).unwrap())
    }
}
