use serde::Serialize;

#[derive(Serialize, Clone, Debug)]
pub struct Parameters {
    pub hue: f32,
    pub smoothness: f32,
    pub metallic: f32,
    pub emission: f32,
}