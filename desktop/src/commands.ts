import { invoke } from "@tauri-apps/api/core";

async function updateParameterValues(parameterName: string, value: Number) {
  await invoke("update_slider", {
    parameterName: parameterName,
    value: value,
  });
}

async function getIpAddress(): Promise<string> {
  return await invoke("get_ip_address");
}

export { updateParameterValues, getIpAddress };
