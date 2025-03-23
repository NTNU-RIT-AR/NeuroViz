// Load files from the folder

import { invoke } from "@tauri-apps/api/core";
import { useEffect, useState } from "react";
import { retrievePreset } from "../commands";
import { type Parameters, type Preset } from "../interfaces";

import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import styles from "./styles/Presets.module.css";

async function fetchFiles(): Promise<string[]> {
  try {
    return await invoke("list_presets");
  } catch (e) {
    console.log("could not fetch files: ", e);
    return Promise.resolve([]);
  }
}
type PresetProps = { name: string };

function Preset({ name }: PresetProps) {
  return (
    <div>
      <p>{name} </p>
      <button className="delete">delete</button>

      <button className="test">test</button>
    </div>
  );
}

// Delete a file
// async function deleteFile(fileName: string) {
//   await invoke('delete_file')
// }

type presetElementProps = {
  name: string;
  onSelect: () => void;
};

function PresetElement({ name, onSelect }: presetElementProps) {
  return (
    <div className={styles.presetElement}>
      <p>{name}</p>
      <div className={styles.buttonsContainer}>
        <button onClick={onSelect}>Select</button>
        <button></button>
      </div>
    </div>
  );
}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);
  const [selectedPreset, setSelectedPreset] = useState<string | undefined>(
    undefined
  );
  const [selectedPresetName, setSelectedPresetName] = useState<string>("");

  const [preset, setPreset] = useState("");

  useEffect(() => {
    fetchFiles().then(setFiles);
  }, []);

  useEffect(() => {
    return () => {};
  }, [preset]);

  return (
    <>
      <h1>Presets page text</h1>

      <Layout>
        <ContentBox className={styles.presetsContainer}>
          {files.map((file) => (
            <PresetElement
              name={file}
              onSelect={() => {
                retrievePreset(file.split(".")[0]).then((result) =>
                  setSelectedPreset(result)
                );
              }}
            />
          ))}
        </ContentBox>

        {/* TODO: Show as sliders */}
        <ContentBox>{selectedPreset}</ContentBox>
      </Layout>
    </>
  );
}
