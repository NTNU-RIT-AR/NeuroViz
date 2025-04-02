import { useEffect, useState } from "react";
import Select from "react-select";
import { useImmer } from "use-immer";
import { commands, ParameterKey, ParameterValues } from "../bindings.gen";
import Button from "../components/Button";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import Popup from "../components/Popup";
import SliderCollection from "../components/SliderCollection";
import { useCommand } from "../hooks";
import styles from "./styles/LiveView.module.css";

/// Fetches the initial live parameters and lets the user update them
function useLiveParameters() {
  const parameters = useCommand(commands.getParameters).data;
  const initialLiveParameters = useCommand(commands.getLiveParameters).data;

  // Initialize the parameters state with zero values
  const [parameterStates, setParameterStates] = useImmer(() =>
    parameters.map((parameter) => ({
      ...parameter,
      value: initialLiveParameters[parameter.key],
    }))
  );

  useEffect(() => {
    const parameters = Object.fromEntries(
      parameterStates.map((parameter) => [parameter.key, parameter.value])
    ) as Record<ParameterKey, number>;

    commands.setLiveParameters(parameters);
  }, [parameterStates]);

  const changeableParameterStates = parameterStates.map((parameter) => ({
    ...parameter,

    onChange(newValue: number) {
      setParameterStates((parameters) => {
        parameters.find((p) => p.key == parameter.key)!.value = newValue;
      });
    },
  }));

  return {
    state: changeableParameterStates,

    set(parameterValues: ParameterValues) {
      setParameterStates((parameters) => {
        for (const parameter of parameters) {
          parameter.value = parameterValues[parameter.key];
        }
      });
    },
  };
}

/// Fetches the presets and lets the user select one
function useSelectPreset() {
  const presets = useCommand(commands.getPresets).data;

  const [selectedPresetKey, setSelectedPresetKey] = useState<
    string | undefined
  >(undefined);

  const selectedPreset = presets.find(
    (preset) => preset.key === selectedPresetKey
  );

  const options = presets.map((preset) => ({
    value: preset.key,
    label: preset.value.name,
  }));

  return {
    selected: selectedPreset,
    options,

    onChange(presetKey: string | undefined) {
      setSelectedPresetKey(presetKey);
    },
  };
}

export default function LiveViewPage() {
  const liveParameters = useLiveParameters();
  const selectPreset = useSelectPreset();

  const [showPresetCreationPopup, setShowPresetCreationPopup] =
    useState<boolean>(false);

  // Update the live parameters when a preset is selected
  useEffect(() => {
    const selectedPreset = selectPreset.selected;

    if (selectedPreset) liveParameters.set(selectedPreset.value.parameters);
  }, [selectPreset.selected]);

  return (
    <Layout title="Live View">
      <ContentBox className={styles.contentBox}>
        <Select
          className={styles.select}
          options={selectPreset.options}
          onChange={(option) => selectPreset.onChange(option?.value)}
        />
        <SliderCollection parameters={liveParameters.state} />

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
