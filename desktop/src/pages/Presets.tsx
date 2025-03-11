// Load files from the folder

import { useEffect, useState } from "react";
import { type Parameters } from "../interfaces";
import { retrievePreset } from "../commands";
import ParametersPreset from "../components/ParametersPreset";
import { invoke } from '@tauri-apps/api/core';

async function fetchFiles(): Promise<string[]> {
  try {
    return await invoke('list_presets');
  } catch (e) {
    console.log("could not fetch files: ", e)
    return Promise.resolve([]);
  }
}

// Delete a file
// async function deleteFile(fileName: string) {
//   await invoke('delete_file')
// }

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);

  useEffect(() => {
    fetchFiles().then(setFiles);
  }, [])

  return (
    <>
      <p>presets page text</p>

      <div className="row">
        <ul className="column">
          {files.map((file) => (
            <button className={selectedPresetName == file.split(".")[0] ? "preset-button active-preset-button" : "preset-button"} onClick={() => {
              getPreset(file.split(".")[0]);
              setSelectedPresetName(file.split(".")[0]);
            }} key={file}>{file.split(".")[0]}</button>
          ))}
        </ul>
        {selectedPreset && <ParametersPreset presetName={selectedPresetName} parameters={selectedPreset} />}
      </div>
    </>
  );
}
