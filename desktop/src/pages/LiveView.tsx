import Select from "react-select";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection";
import { useEffect, useMemo, useState } from "react";
import { capitalizeFirstLetter } from "../utils";
import Slider from "../components/Slider";
import { Parameter, ParameterWithValue } from "../interfaces";
import { useImmer } from "use-immer"
import { commands, Preset } from "../bindings.gen";

function usePresetKeys(): string[] {
  const [presetKeys, setPresets] = useState<string[]>([]);


  useEffect(() => {
    commands.listPresets().then(setPresets);
  }, []);

  return presetKeys
}

function usePreset(presetKey: string | undefined): Preset | undefined {
  const [preset, setPreset] = useState<Preset | undefined>()

  useEffect(() => {
    if (presetKey) {
      commands.getPreset(presetKey).then(setPreset);
    }
  }, [presetKey])


  return preset
}

export default function LiveViewPage() {
  const presetKeys = usePresetKeys();
  const [selectedPresetKey, setSelectedPresetKey] = useState<string | undefined>();
  const preset = usePreset(selectedPresetKey);

  const [parameters, setParameters] = useImmer<ParameterWithValue[]>([]);

  useEffect(() => {
    commands.getParameters().then(async (parameters) => {
      const promises = parameters.map(async (parameter) => {
        const liveParameter = await commands.getLiveParameter(parameter.key);
        return {
          ...parameter,
          value: liveParameter
        }
      });

      setParameters(await Promise.all(promises))
    });
  }, []);

  useEffect(() => {
    for (const parameter of parameters) {
      commands.setLiveParameter(parameter.key, parameter.value);
    }
  }, [parameters])

  useEffect(() => {
    if (preset) {
      setParameters((parameters) => {
        for (const parameter of parameters) {
          parameter.value = preset.parameters[parameter.key]!;
        }
      })
    }
  }, [preset])


  const sliderParameters = parameters.map(parameter => ({
    ...parameter,

    onChange(newValue: number) {
      setParameters(parameters => {
        parameters.find(p => p.key == parameter.key)!.value = newValue;
      })
    }
  }))

  const options = presetKeys.map((presetKey: string) => ({
    value: presetKey,
    label: capitalizeFirstLetter(presetKey)
  }
  ));

  // to get param 
  // invoke<number>("get_live_parameter", { parameter: name }).then(setValue);

  // to set param ( do this every onChange )
  // setValue(newValue);
  // invoke("set_live_parameter", { parameter: name, value: newValue });

  // onChange
  // (e) => {
  //   handleChange(
  //     Math.min(Math.max(parseFloat(e.target.value), min), max)
  //   );
  // }


  return (
    <Layout title="Live View">
      <ContentBox>

        <Select options={options} onChange={(option) => setSelectedPresetKey(option?.value)} />

        <SliderCollection parameters={sliderParameters} />
      </ContentBox>
    </Layout>
  );
}
