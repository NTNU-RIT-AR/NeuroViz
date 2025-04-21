use serde::{Deserialize, Serialize};
use specta::Type;

use super::parameters::ParameterValues;

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct Preset {
    pub name: String,
    pub parameters: ParameterValues,
}
