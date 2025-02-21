import { invoke } from "@tauri-apps/api/core";

async function updateParameterValues(slider: String, value: Number) {
  await invoke("update_slider", {
    sliderNumber: slider,
    sliderValue: value,
  });
}

export { updateParameterValues };
