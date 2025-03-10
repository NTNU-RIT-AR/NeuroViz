use serde::Serialize;

#[derive(Serialize, Clone, Debug)]
pub struct Parameters {
    pub slider1: f32,
    pub slider2: f32,
    pub slider3: f32,
    pub slider4: f32,
}