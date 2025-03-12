use std::sync::Mutex;

use serde::Serialize;

#[derive(Serialize, Clone, Debug, Default)]
pub struct RenderParamsInner {
    pub hue: f32,
    pub smoothness: f32,
    pub metallic: f32,
    pub emission: f32,
}

pub type RenderParams = Mutex<RenderParamsInner>;
