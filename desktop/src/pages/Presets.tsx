import { EyeIcon, TrashIcon } from "@heroicons/react/24/outline";
import { useState } from "react";
import { commands, WithKey, type Preset } from "../bindings.gen.ts";
import Button from "../components/Button.tsx";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection.tsx";
import { useCommand } from "../hooks.ts";
import styles from "./styles/Presets.module.css";

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
          <EyeIcon />
        </Button>
        <Button
          variant="danger"
          onClick={() => {
            if (confirm("Are you sure you want to delete this preset?")) {
              onDelete();
            }
          }}
          square={true}
        >
          <TrashIcon />
        </Button>
      </div>
    </div>
  );
}

interface PresetPreviewProps {
  preset: Preset;
}

function PresetPreview(props: PresetPreviewProps) {
  const { preset } = props;
  const parameters = useCommand(commands.getParameters).data;

  const parameterStates = parameters.map((parameter) => ({
    ...parameter,
    value: preset.parameters[parameter.key],
  }));

  return <SliderCollection parameters={parameterStates} />;
}

export default function PresetsPage() {
  const presets = useCommand(commands.getPresets);

  const [selectedPreset, setSelectedPreset] = useState<
    WithKey<Preset> | undefined
  >(undefined);

  async function deletePreset(presetKey: string) {
    await commands.deletePreset(presetKey);
    presets.refetch();
  }

  return (
    <Layout title="Presets">
      <ContentBox className={styles.presetsContainer}>
        {presets.data.map((preset) => (
          <PresetElement
            name={preset.value.name}
            onDelete={() => deletePreset(preset.key)}
            onSelect={() => setSelectedPreset(preset)}
          />
        ))}
      </ContentBox>

      <ContentBox>
        {selectedPreset && <PresetPreview preset={selectedPreset.value} />}
      </ContentBox>
    </Layout>
  );
}
