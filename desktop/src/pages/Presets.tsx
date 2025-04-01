// Load files from the folder
import { invoke } from "@tauri-apps/api/core";
import { useEffect, useState } from "react";
import { retrievePreset } from "../commands";
import { type Parameter, type Preset } from "../interfaces";

import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import styles from "./styles/Presets.module.css";
import Button from "../components/Button.tsx";

import { EyeIcon, TrashIcon } from "@heroicons/react/24/outline"
import SliderCollection from "../components/SliderCollection.tsx";


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

      <button className="test"></button>
    </div>
  );
}

// Delete a file
// async function deleteFile(fileName: string) {
//   await invoke('delete_file', {name: filename})
// }

type presetElementProps = {
  name: string;
  onSelect: () => void;
};

function PresetElement({ name, onSelect }: presetElementProps) {


  return (
    <div className={styles.presetElement} onClick={onSelect}>
      <p>{name}</p>
      <div className={styles.buttonsContainer}>
        <Button onClick={onSelect} square={true}>
          <EyeIcon className="icon" />
        </Button>
        <Button square={true}>
          <TrashIcon className={`icon ${styles.trashIcon}`} />
        </Button>
      </div>
    </div>
  );
}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);
  const [selectedPreset, setSelectedPreset] = useState<string | undefined>(undefined);
  const [preset, setPreset] = useState<Preset | undefined>(undefined);

  useEffect(() => {
    fetchFiles().then(setFiles);
  }, []);

  useEffect(() => {
    return () => { };
  }, [preset]);

  return (
    <>
      <Layout title="Presets">
        <ContentBox className={styles.presetsContainer}>
          {files.map((file) => (
            <PresetElement
              name={file}
              onSelect={() => {
                invoke<Preset>("get_preset").then(setPreset)
              }}
            />
          ))}
        </ContentBox>

        {/* TODO: Show as sliders */}
        <ContentBox>
          {/* {selectedPreset} */}
          {
            preset &&
            <SliderCollection parameters={preset.parameters} />
          }

        </ContentBox>
      </Layout>
    </>
  );
}
