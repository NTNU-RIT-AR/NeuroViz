#[macro_export]
macro_rules! define_parameters {
    ($(
        $variant:ident => {
            key: $key:ident,
            name: $name:expr,
            min: $min:expr,
            max: $max:expr,
            default: $default:expr
        }
    ),* $(,)?) => {
        use std::fmt::Display;
        use serde::{Deserialize, Serialize};
        use specta::Type;
        use strum::{EnumIter, IntoEnumIterator};

        #[derive(Deserialize, Serialize, Type, Clone, Copy, Debug, PartialEq, Hash, Eq, EnumIter)]
        #[serde(rename_all = "snake_case")]
        pub enum ParameterKey {
            $($variant),*
        }

        impl Display for ParameterKey {
            fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
                match self {
                    $(
                        ParameterKey::$variant => write!(f, "{}", stringify!($key)),
                    )*
                }
            }
        }

        #[derive(Deserialize, Serialize, Type, Clone, Debug, PartialEq)]
        pub struct Parameter {
            pub key: ParameterKey,
            pub name: &'static str,
            pub min: f32,
            pub max: f32,
            pub default: f32,
        }

        impl ParameterKey {
            pub fn parameter_for(self) -> Parameter {
                match self {
                    $(
                        ParameterKey::$variant => Parameter {
                            key: self,
                            name: $name,
                            min: $min,
                            max: $max,
                            default: $default,
                        }
                    ),*
                }
            }
        }

        impl Parameter {
            pub fn all() -> impl Iterator<Item = Parameter> {
                ParameterKey::iter().map(|key| key.parameter_for())
            }
        }

        #[derive(Deserialize, Serialize, Type, Clone, Copy, Debug, PartialEq)]
        pub struct ParameterValues {
            $(pub $key: f32),*
        }

        impl ParameterValues {
            pub fn get(&self, param: ParameterKey) -> f32 {
                match param {
                    $(ParameterKey::$variant => self.$key),*
                }
            }

            pub fn set(&mut self, param: ParameterKey, value: f32) {
                match param {
                    $(ParameterKey::$variant => self.$key = value),*
                }
            }
        }

        impl Default for ParameterValues {
            fn default() -> Self {
                Self {
                    $($key: $default),*
                }
            }
        }
    };
}
