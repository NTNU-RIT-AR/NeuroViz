import { EyeIcon, TrashIcon } from "@heroicons/react/24/outline";
import classNames from "classnames";
import { useState, useMemo } from "react";
import { commands, WithKey, type Preset } from "../bindings.gen.ts";
import Button from "../components/Button.tsx";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection.tsx";
import { useCommand } from "../hooks.ts";
import { Input } from "../components/Input.tsx";
import Fuse from "fuse.js";
import styles from "./Presets.module.css";

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

  const [search, setSearch] = useState('');

  const fuse = useMemo(() => new Fuse(presets.data, {
    keys: ['value.name'],
    threshold: 0.3,
  }), [presets.data]);

  const filteredPresets = useMemo(() => {
    if (!search.trim()) return presets.data;
    return fuse.search(search).map(res => res.item);
  }, [search, fuse]);

  async function deletePreset(presetKey: string) {
    await commands.deletePreset(presetKey);
    presets.refetch();
  }

  return (
    <Layout title="Presets" folder="Presets" className={styles.container}>
      <ContentBox
        className={classNames(styles.contentBox, styles.presetsContainer)}
        role="listbox"
      >

        <div className={styles.headerRow}>
          <h2 className="title">Presets</h2>
          <div className={styles.searchBoxWrapper}>
            <Input
              placeholder="Search presets"
              value={search}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSearch(e.target.value)}
            />
          </div>
        </div>


        {filteredPresets.map((preset) => (
          <div
            key={preset.key}
            className={classNames(
              styles.presetElement,
              selectedPreset?.key === preset.key && styles.presetElementSelected,
            )}
            onClick={() => setSelectedPreset(preset)}
            role="option"
            aria-selected={selectedPreset?.key === preset.key}
          >
            <p>{preset.value.name}</p>
            <div className={styles.buttonsContainer}>
              <Button
                variant="danger"
                onClick={() => {
                  if (confirm("Are you sure you want to delete this preset?")) {
                    deletePreset(preset.key);
                  }
                }}
                square={true}
              >
                <TrashIcon />
              </Button>
            </div>
          </div>
        ))}
      </ContentBox>

      <ContentBox className={styles.contentBox}>
        {selectedPreset && <PresetPreview preset={selectedPreset.value} />}
      </ContentBox>
    </Layout>
  );
}
