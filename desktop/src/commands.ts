import { invoke } from "@tauri-apps/api/core";
import { Preset, Experiment, Parameters } from "./interfaces";

async function updateParameterValues(parameterName: string, value: Number) {
  await invoke("update_slider", {
    parameterName: parameterName,
    value: value,
  });
}

async function getIpAddress(): Promise<string> {
  return await invoke("get_ip_address");
}

async function retrievePreset(
  sluggedPresetName: string
): Promise<string | undefined> {
  return await invoke("get_preset", {
    sluggedPresetName,
  })
    .then((result) => {
      console.log(result);
      //TODO: Parse objektet "result" som er returnert fra command invocation
      return JSON.stringify(result);
    })
    .catch((err) => {
      alert(err);
      return undefined;
    });
}

async function retrieveExperiment(
  sluggedExperimentName: string
): Promise<string | undefined> {
  return await invoke("retrieve_experiment", {
    sluggedExperimentName,
  })
    .then((result) => {
      console.log(result);
      //TODO: Parse objektet "result" som er returnert fra command invocation
      return JSON.stringify(result);
    })
    .catch((err) => {
      alert(err);
      return undefined;
    });
}

async function fetchExperiments(): Promise<string[]> {
  try {
    return await invoke("list_experiments");
  } catch (e) {
    console.log(e);
    return Promise.resolve([]);
  }
}

// function parsePreset(preset: string): Parameters | undefined {
//   //Parse the JSON object
//   try {
//     const parsedObject: Partial<Parameters> = JSON.parse(preset);

//     const parameters: Parameters = {
//       hue: parsedObject.hue ?? 1.0,
//       smoothness: parsedObject.smoothness ?? 1.0,
//       metallic: parsedObject.metallic ?? 1.0,
//       emission: parsedObject.emission ?? 1.0,
//     };
//     return parameters;
//   } catch (error) {
//     return undefined;
//   }
// }

export {
  updateParameterValues,
  getIpAddress,
  retrievePreset,
  retrieveExperiment,
  fetchExperiments,
};
