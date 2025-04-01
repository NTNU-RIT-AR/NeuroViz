import Select from "react-select";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection";
import { useEffect, useMemo, useState } from "react";
import { capitalizeFirstLetter } from "../utils";
import Slider from "../components/Slider";
import { Parameter } from "../interfaces";
import { useImmer } from "use-immer"
import { commands } from "../bindings.gen";

interface ParameterWithValue extends Parameter {
  value: number
}

export default function LiveViewPage() {

  const [selectedPreset, setSelectedPreset] = useState<string | undefined>();

  const [presetKeys, setPresets] = useState<string[]>([]);

  const [parameters, setParameters] = useImmer<ParameterWithValue[]>([]);

  useEffect(() => {
    commands.listPresets().then(setPresets);

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
    if (selectedPreset) {
      commands.getPreset(selectedPreset).then((preset) => {
        console.log(selectedPreset);

        setParameters((parameters) => {
          for (const parameter of parameters) {
            parameter.value = preset.parameters[parameter.key]!;
          }
        })
      });

    }
  }, [selectedPreset])


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

        <Select options={options} onChange={(option) => setSelectedPreset(option?.value)} />

        <SliderCollection parameters={sliderParameters} />
      </ContentBox>
    </Layout>
  );
}
