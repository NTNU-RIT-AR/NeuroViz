import { invoke } from "@tauri-apps/api/core";

async function updateParameterValues(slider: String, value: Number) {
  await invoke("update_slider", {
    sliderNumber: slider,
    sliderValue: value,
  });
}

async function getIpAddress(): Promise<string> {
  return await invoke("get_ip_address");
}

export { updateParameterValues, getIpAddress };
