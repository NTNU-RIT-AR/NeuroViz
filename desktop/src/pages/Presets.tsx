// Load files from the folder
import { EyeIcon, TrashIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import { useImmer } from "use-immer";
import { commands, WithKey, type Preset } from "../bindings.gen.ts";
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
  const [presets, setPresets] = useState<WithKey<Preset>[]>([]);
  const [selectedPreset, setSelectedPreset] = useState<
    WithKey<Preset> | undefined
  >(undefined);

  const [parameters, setParameters] = useImmer<ParameterWithValue[]>([]);

  useEffect(() => {
    commands.getAllPresets().then(setPresets);
    commands.getParameters().then((parameters) =>
      setParameters(
        parameters.map((parameter) => ({
          ...parameter,
          value: 0,
        })),
      ),
    );
  }, []);

  useEffect(() => {
    if (selectedPreset) {
      setParameters((parameters) => {
        for (const parameter of parameters) {
          parameter.value = selectedPreset.value.parameters[parameter.key];
        }
      });
    }
  }, [selectedPreset]);

  return (
    <Layout title="Presets">
      <ContentBox className={styles.presetsContainer}>
        {presets.map((preset) => (
          <PresetElement
            name={preset.value.name}
            onDelete={async () => {
              // if (selectedPreset === preset.value) {
              //   setSelectedPreset(undefined);
              // }
              await commands.deletePreset(preset.key);
              //TODO: just remove from frontend state, instead of fetching
              commands.getAllPresets().then(setPresets);
            }}
            onSelect={() => {
              setSelectedPreset(preset);
            }}
          />
        ))}
      </ContentBox>

      <ContentBox>
        {selectedPreset && (
          <SliderCollection
            parameters={parameters.map((parameter) => ({
              ...parameter,
              onChange: (_: number) => {},
            }))}
          />
        )}
      </ContentBox>
    </Layout>
  );
}
