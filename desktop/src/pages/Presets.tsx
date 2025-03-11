// Load files from the folder

import { useEffect, useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { type Parameters } from "../interfaces";
import { retrievePreset } from "../commands";
import ParametersPreset from "../components/ParametersPreset";

async function fetchFiles(folder: String): Promise<string[]> {
  try {
    return await invoke("list_files", { folder: folder });
  } catch (error) {
    console.log("error");
    return Promise.resolve([])
  }
}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);
  const [selectedPreset, setSelectedPreset] = useState<Parameters | undefined>(undefined);
  const [selectedPresetName, setSelectedPresetName] = useState<string>("");

  // Delete a file
  // async function deleteFile(fileName: string) {
  //   await invoke('delete_file')
  // }
  async function getPreset(presetName: string) {
    setSelectedPreset(await retrievePreset(presetName));
  }

  useEffect(() => {
    let folder = "/presets";
    try {
      fetchFiles(folder);
      console.log("success");
    } catch (error) {
      console.log("error");
    }

    fetchFiles(folder);

    fetchFiles(folder).then(setFiles);

  }, []);

  return (
    <>
      <p>presets page text</p>
      
      <div className="row">
        <ul className="column">
          {files.map((file) => (
            <button className={selectedPresetName == file.split(".")[0] ? "preset-button active-preset-button" : "preset-button"} onClick={()=>{
              getPreset(file.split(".")[0]);
              setSelectedPresetName(file.split(".")[0]);
            }} key={file}>{file.split(".")[0]}</button>
          ))}
        </ul>
        {selectedPreset && <ParametersPreset presetName={selectedPresetName} parameters={selectedPreset}/>}
      </div>
    </>
  );
}
