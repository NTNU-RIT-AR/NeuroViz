use serde::{Deserialize, Serialize};
use specta::Type;
use strum::{EnumIter, IntoEnumIterator};

#[derive(Deserialize, Serialize, Type, Clone, Copy, Debug, PartialEq, Hash, Eq, EnumIter)]
#[serde(rename_all = "snake_case")]
pub enum ParameterKey {
    Transparency,
    Outline,
    Smoothness,
    LightIntensity,
    LightTemperature,
}

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct Parameter {
    key: ParameterKey,
    name: &'static str,
    min: f32,
    max: f32,
    default: f32,
}

impl ParameterKey {
    /// Get the parameter data for this parameter key
    pub fn parameter_for(self) -> Parameter {
        let key = self;

        use ParameterKey::*;
        match self {
            Transparency => Parameter {
                key,
                name: "Transparency",
                min: 0.0,
                max: 1.0,
                default: 0.,
            },
            Outline => Parameter {
                key,
                name: "Outline",
                min: 0.0,
                max: 1.0,
                default: 0.,
            },
            Smoothness => Parameter {
                key,
                name: "Smoothness",
                min: 0.0,
                max: 1.0,
                default: 0.,
            },
            LightIntensity => Parameter {
                key,
                name: "Light Intensity",
                min: 0.0,
                max: 2.0,
                default: 1.,
            },
            LightTemperature => Parameter {
                key,
                name: "Light Temperature",
                min: 1500.,
                max: 20000.,
                default: 6500.,
            },
        }
    }
}

impl Parameter {
    pub fn all() -> impl Iterator<Item = Parameter> {
        ParameterKey::iter().map(|key| key.parameter_for())
    }
}

#[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
pub struct ParameterValues {
    pub transparency: f32,
    pub outline: f32,
    pub smoothness: f32,
    pub light_intensity: f32,
    pub light_temperature: f32,
}

impl ParameterValues {
    pub fn get(&self, param: ParameterKey) -> f32 {
        match param {
            ParameterKey::Transparency => self.transparency,
            ParameterKey::Outline => self.outline,
            ParameterKey::Smoothness => self.smoothness,
            ParameterKey::LightIntensity => self.light_intensity,
            ParameterKey::LightTemperature => self.light_temperature,
        }
    }

    pub fn set(&mut self, param: ParameterKey, value: f32) {
        match param {
            ParameterKey::Transparency => self.transparency = value,
            ParameterKey::Outline => self.outline = value,
            ParameterKey::Smoothness => self.smoothness = value,
            ParameterKey::LightIntensity => self.light_intensity = value,
            ParameterKey::LightTemperature => self.light_temperature = value,
        }
    }
}

impl Default for ParameterValues {
    fn default() -> Self {
        fn default_for(key: ParameterKey) -> f32 {
            key.parameter_for().default
        }

        Self {
            transparency: default_for(ParameterKey::Transparency),
            outline: default_for(ParameterKey::Outline),
            smoothness: default_for(ParameterKey::Smoothness),
            light_intensity: default_for(ParameterKey::LightIntensity),
            light_temperature: default_for(ParameterKey::LightTemperature),
        }
    }
}

#[cfg(test)]
mod tests {
    use std::collections::HashMap;

    use super::*;

    /// Tests that all parameters are listed in Parameter::all()
    #[test]
    fn test_all_is_correct() {
        let mut all_parameter_values = HashMap::new();

        for param in Parameter::all() {
            let json_key = serde_json::to_string(&param.key).unwrap();
            let unquoted_key = json_key.trim_matches('"');

            all_parameter_values.insert(unquoted_key.to_owned(), param.default);
        }

        // Serialize HashMap to json and serialize back as ParameterValues
        let all_parameter_values: ParameterValues =
            serde_json::from_str(&serde_json::to_string(&all_parameter_values).unwrap()).unwrap();
        let correct_parameter_values = ParameterValues::default();

        assert_eq!(all_parameter_values, correct_parameter_values);
    }
}
