// Load files from the folder

import { useEffect, useState } from "react";
import { invoke } from '@tauri-apps/api/core';

import styles from "./styles/Presets.module.css";
import SliderCollection from "../components/SliderCollection";

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

type presetElementProps = {
  name: string
}

function PresetElement({ name }: presetElementProps) {
  return (
    <div className={styles.presetElement}>
      <p>{name}</p>
      <div className={styles.buttonsContainer}>
        <button></button>
        <button></button>
      </div>
    </div>
  )
}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);

  const [preset, setPreset] = useState("");

  useEffect(() => {
    fetchFiles().then(setFiles);
  }, [])

  useEffect(() => {

    return () => {

    };
  }, [preset]);

  return (
    <>
      <h1>Presets page text</h1>
      <div className="layoutContainer">

        <div className={`${styles.presetsContainer} contentBox`}>
          {files.map((file) => (
            <PresetElement name={file} />
          ))}
        </div>

        <div className="contentBox">
          <SliderCollection />
        </div>
      </div>
    </>

  )
}
