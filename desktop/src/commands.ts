import { invoke } from "@tauri-apps/api/core";

async function updateParameterValues(
  s1: Number,
  s2: Number,
  s3: Number,
  s4: Number
) {
  await invoke("update_slider", {
    newSlider1: s1,
    newSlider2: s2,
    newSlider3: s3,
    newSlider4: s4,
  });
}

export { updateParameterValues };
