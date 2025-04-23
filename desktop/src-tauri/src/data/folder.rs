use serde::{Deserialize, Serialize};
use specta::Type;

#[derive(Debug, Deserialize, Serialize, Type, Clone)]
pub enum TopLevelFolder {
    Presets,
    Experiments,
    Results,
}

impl TopLevelFolder {
    pub fn path(&self) -> String {
        match self {
            TopLevelFolder::Presets => "presets",
            TopLevelFolder::Experiments => "experiments",
            TopLevelFolder::Results => "results",
        }
        .to_string()
    }
}
