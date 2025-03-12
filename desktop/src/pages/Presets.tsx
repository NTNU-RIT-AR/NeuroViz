// Load files from the folder

import { useEffect, useState } from "react";
import { type Parameters } from "../interfaces";
import { retrievePreset } from "../commands";
import ParameterControl from "../components/ParametersControl";
import { invoke } from '@tauri-apps/api/core';

async function fetchFiles(): Promise<string[]> {
  try {
    return await invoke('list_presets');
  } catch (e) {
    console.log("could not fetch files: ", e)
    return Promise.resolve([]);
  }
}
type PresetProps = { name: string }

function Preset({ name }: PresetProps) {

  return (
    <div>
      <p>{name} </p>
      <button className="delete">
        delete
      </button>

      <button className="test">
        test
      </button>

    </div>
  )
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
      <h1>Presets page</h1>


      <div className="two-column-layout">
        <div className="container">
          {files.map((file) => (
            <Preset name={file}></Preset>
          ))}
        </div>
        <div className="container">

        </div>
      </div >

      {/* <div className="row"> */}
      {/*   <ul className="column"> */}
      {/*     <button className={selectedPresetName == file.split(".")[0] ? "preset-button active-preset-button" : "preset-button"} onClick={() => { */}
      {/*       getPreset(file.split(".")[0]); */}
      {/*       setSelectedPresetName(file.split(".")[0]); */}
      {/*     }} key={file}>{file.split(".")[0]}</button> */}
      {/*     ))} */}
      {/*   </ul> */}
      {/*   {selectedPreset && <ParameterControl presetName={selectedPresetName} parameters={selectedPreset} />} */}
      {/* </div> */}
    </>
  );
}
