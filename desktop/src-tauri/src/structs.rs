use std::sync::{Mutex, MutexGuard};

use serde::{Deserialize, Serialize};

use tauri::{AppHandle, Manager};

#[derive(Serialize, Deserialize, Clone, Debug, Default, PartialEq)]
pub struct RenderParamsInner {
    pub hue: f32,
    pub smoothness: f32,
    pub metallic: f32,
    pub emission: f32,
}

pub type RenderParams = Mutex<RenderParamsInner>;

//TODO: implement something like this for easier access to renderparams
//pub fn aquire_render_params<'a>(app: &'a AppHandle) -> MutexGuard<'a, RenderParamsInner> {
//    let state = app.state::<RenderParams>();
//    state.lock().unwrap()
//}
