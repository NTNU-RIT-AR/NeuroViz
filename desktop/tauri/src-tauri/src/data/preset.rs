use neuroviz::parameters::ParameterValues;
use serde::{Deserialize, Serialize};
use specta::Type;

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct Preset {
    pub name: String,
    pub parameters: ParameterValues,
}
