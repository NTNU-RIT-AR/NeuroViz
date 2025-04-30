import { cache, use, useEffect, useRef, useState } from "react";
import { useImmer } from "use-immer";
import { commands, ParameterKey, ParameterValues } from "../bindings.gen";
import Button from "../components/Button";
import { ContentBox } from "../components/ContentBox";
import { Input, Label, Select } from "../components/Input";
import { Layout } from "../components/Layout";
import Popup from "../components/Popup";
import SliderCollection from "../components/SliderCollection";
import { useCommand } from "../hooks";
import styles from "./LiveView.module.css";
import { useSuspenseQuery } from "@tanstack/react-query";

/// Fetches the initial live parameters and lets the user update them
export function useLiveParameters() {
  const parameters = useCommand(commands.getParameters).data;
  const initialLiveParameters = useCommand(commands.getLiveParameters).data;

  // Initialize the parameters state with zero values
  const [parameterStates, setParameterStates] = useImmer(() =>
    parameters.map((parameter) => ({
      ...parameter,
      value: initialLiveParameters[parameter.key],
    })),
  );

  useEffect(() => {
    const parameters = Object.fromEntries(
      parameterStates.map((parameter) => [parameter.key, parameter.value]),
    ) as Record<ParameterKey, number>;

    console.log(parameters);

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
  const presets = useCommand(commands.getPresets);

  const [selectedPresetKey, setSelectedPresetKey] = useState<
    string | undefined
  >(undefined);

  const selectedPreset = presets.data.find(
    (preset) => preset.key === selectedPresetKey,
  );

  const options = presets.data.map((preset) => ({
    value: preset.key,
    label: preset.value.name,
  }));

  return {
    selected: selectedPreset,
    refetch: presets.refetch,
    options,

    onChange(presetKey: string | undefined) {
      setSelectedPresetKey(presetKey);
    },
  };
}

interface LiveViewPageProps {
  liveParameters: ReturnType<typeof useLiveParameters>;
}

export default function LiveViewPage(props: LiveViewPageProps) {
  const { liveParameters } = props;

  const selectPreset = useSelectPreset();

  const [showPresetCreationPopup, setShowPresetCreationPopup] = useState(false);

  // Update the live parameters when a preset is selected
  useEffect(() => {
    const selectedPreset = selectPreset.selected;

    if (selectedPreset) liveParameters.set(selectedPreset.value.parameters);
  }, [selectPreset.selected]);

  const presetNameRef = useRef<HTMLInputElement>(null);

  return (
    <Layout title="Live View">
      <ContentBox className={styles.contentBox}>
        <Select
          options={selectPreset.options}
          onChange={(option) => selectPreset.onChange(option?.value)}
        />

        <SliderCollection parameters={liveParameters.state} />

        <Button onClick={() => setShowPresetCreationPopup(true)}>
          Save as new preset
        </Button>

        {showPresetCreationPopup && (
          <Popup
            onClose={() => setShowPresetCreationPopup(false)}
            title="Save as new preset"
          >
            <div className={styles.inputFields}>
              <Label>
                Preset name
                <Input
                  type="text"
                  placeholder="preset-name"
                  ref={presetNameRef}
                />
              </Label>

              <Button
                variant="primary"
                onClick={async () => {
                  await commands
                    .createPreset(presetNameRef.current!.value)
                    .then(() => {
                      selectPreset.refetch();
                      setShowPresetCreationPopup(false);
                    })
                    .catch(alert);
                }}
              >
                Save
              </Button>
            </div>
          </Popup>
        )}
      </ContentBox>
    </Layout>
  );
}
