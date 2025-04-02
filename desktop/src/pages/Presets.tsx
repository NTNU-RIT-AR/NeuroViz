// Load files from the folder
import { EyeIcon, TrashIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import { useImmer } from "use-immer";
import { commands, type Preset } from "../bindings.gen.ts";
import Button from "../components/Button.tsx";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import { ParameterWithValue } from "../interfaces.ts";
import styles from "./styles/Presets.module.css";
import SliderCollection from "../components/SliderCollection.tsx";

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

type presetElementProps = {
  name: string;
  onSelect: () => void;
  onDelete: () => void;
};

function PresetElement({ name, onSelect, onDelete }: presetElementProps) {
  return (
    <div className={styles.presetElement} onClick={onSelect}>
      <p>{name}</p>
      <div className={styles.buttonsContainer}>
        <Button onClick={onSelect} square={true}>
          <EyeIcon className="icon" />
        </Button>
        <Button onClick={onDelete} square={true}>
          <TrashIcon className={`icon ${styles.trashIcon}`} />
        </Button>
      </div>
    </div>
  );
}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);
  const [selectedPreset, setSelectedPreset] = useState<string | undefined>(
    undefined,
  );
  const [preset, setPreset] = useState<Preset | undefined>(undefined);

  const [parameters, setParameters] = useImmer<ParameterWithValue[]>([]);

  useEffect(() => {
    commands.listPresets().then(setFiles);
  }, []);

  useEffect(() => {
    return () => {};
  }, [preset]);

  return (
    <Layout title="Presets">
      <ContentBox className={styles.presetsContainer}>
        {files.map((file) => (
          <PresetElement
            name={file}
            onDelete={async () => {
              await commands.deletePreset(file);
              commands.listPresets().then(setFiles);
            }}
            onSelect={() => {
              commands.getPreset(file).then(setPreset);
            }}
          />
        ))}
      </ContentBox>

      {/* TODO: Show as sliders */}
      <ContentBox>
        {/* {selectedPreset} */}
        {/* {preset && <SliderCollection parameters={preset.parameters} />} */}
      </ContentBox>
    </Layout>
  );
}
