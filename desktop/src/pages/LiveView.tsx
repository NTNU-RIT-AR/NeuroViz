import { useEffect, useState } from "react";
import Select from "react-select";
import { useImmer } from "use-immer";
import { commands, Preset } from "../bindings.gen";
import Button from "../components/Button";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import Popup from "../components/Popup";
import SliderCollection from "../components/SliderCollection";
import { ParameterWithValue } from "../interfaces";
import { capitalizeFirstLetter } from "../utils";
import styles from "./styles/LiveView.module.css";

function usePresetKeys(): string[] {
  const [presetKeys, setPresets] = useState<string[]>([]);

  useEffect(() => {
    commands.listPresets().then(setPresets);
  }, []);

  return presetKeys;
}

function usePreset(presetKey: string | undefined): Preset | undefined {
  const [preset, setPreset] = useState<Preset | undefined>();

  useEffect(() => {
    if (presetKey) {
      commands.getPreset(presetKey).then(setPreset);
    }
  }, [presetKey]);

  return preset;
}

export default function LiveViewPage() {
  const presetKeys = usePresetKeys();
  const [selectedPresetKey, setSelectedPresetKey] = useState<
    string | undefined
  >();
  const preset = usePreset(selectedPresetKey);

  const [parameters, setParameters] = useImmer<ParameterWithValue[]>([]);

  const [showPresetCreationPopup, setShowPresetCreationPopup] =
    useState<boolean>(false);

  useEffect(() => {
    commands.getParameters().then(async (parameters) => {
      const promises = parameters.map(async (parameter) => {
        const liveParameter = await commands.getLiveParameter(parameter.key);
        return {
          ...parameter,
          value: liveParameter,
        };
      });

      setParameters(await Promise.all(promises));
    });
  }, []);

  useEffect(() => {
    for (const parameter of parameters) {
      commands.setLiveParameter(parameter.key, parameter.value);
    }
  }, [parameters]);

  useEffect(() => {
    if (preset) {
      setParameters((parameters) => {
        for (const parameter of parameters) {
          parameter.value = preset.parameters[parameter.key];
        }
      });
    }
  }, [preset]);

  const sliderParameters = parameters.map((parameter) => ({
    ...parameter,

    onChange(newValue: number) {
      setParameters((parameters) => {
        parameters.find((p) => p.key == parameter.key)!.value = newValue;
      });
    },
  }));

  const options = presetKeys.map((presetKey: string) => ({
    value: presetKey,
    label: capitalizeFirstLetter(presetKey),
  }));

  return (
    <Layout title="Live View">
      <ContentBox className={styles.contentBox}>
        <Select
          className={styles.select}
          options={options}
          onChange={(option) => setSelectedPresetKey(option?.value)}
        />
        <SliderCollection parameters={sliderParameters} />

        <Button
          onClick={() => {
            setShowPresetCreationPopup(true);
          }}
        >
          Save as new preset
        </Button>
        {showPresetCreationPopup && (
          <Popup
            onClose={() => {
              setShowPresetCreationPopup(false);
            }}
            title="Enter preset name"
          >
            <input placeholder="preset-name" />
            <Button>Save</Button>
          </Popup>
        )}
      </ContentBox>
    </Layout>
  );
}
