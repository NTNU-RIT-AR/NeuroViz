use serde::{Deserialize, Serialize};
use specta::Type;

#[derive(Deserialize, Serialize, Type, Clone, Copy, Debug, PartialEq, Hash, Eq)]
pub enum ParameterKey {
    #[serde(rename = "transparency")]
    Transparency,
    #[serde(rename = "see_through")]
    SeeThrough,
    #[serde(rename = "outline")]
    Outline,
    #[serde(rename = "smoothness")]
    Smoothness,
}

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct Parameter {
    key: ParameterKey,
    name: String,
}

impl Parameter {
    pub fn all() -> Vec<Parameter> {
        [
            Parameter {
                key: ParameterKey::Transparency,
                name: "Transparency".to_owned(),
            },
            Parameter {
                key: ParameterKey::SeeThrough,
                name: "See through".to_owned(),
            },
            Parameter {
                key: ParameterKey::Outline,
                name: "Outline".to_owned(),
            },
            Parameter {
                key: ParameterKey::Smoothness,
                name: "Smoothness".to_owned(),
            },
        ]
        .to_vec()
    }
}

#[derive(Deserialize, Serialize, Type, Default, Clone, Copy, Debug, PartialEq)]
pub struct ParameterValues {
    pub transparency: f32,
    pub see_through: f32,
    pub outline: f32,
    pub smoothness: f32,
}

impl ParameterValues {
    pub fn get(&self, param: ParameterKey) -> f32 {
        match param {
            ParameterKey::Transparency => self.transparency,
            ParameterKey::SeeThrough => self.see_through,
            ParameterKey::Outline => self.outline,
            ParameterKey::Smoothness => self.smoothness,
        }
    }

    pub fn set(&mut self, param: ParameterKey, value: f32) {
        match param {
            ParameterKey::Transparency => self.transparency = value,
            ParameterKey::SeeThrough => self.see_through = value,
            ParameterKey::Outline => self.outline = value,
            ParameterKey::Smoothness => self.smoothness = value,
        }
    }
}
