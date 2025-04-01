import Select from "react-select";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection";
import { useEffect, useMemo, useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { capitalizeFirstLetter } from "../utils";
import Slider from "../components/Slider";
import { Parameter } from "../interfaces";
import { useImmer } from "use-immer"


interface ParameterWithValue extends Parameter {
  value: number
}


export default function LiveViewPage() {

  const [selectedPreset, setSelectedPreset] = useState<string | undefined>();

  const [presets, setPresets] = useState<string[]>([]);

  const [parameters, setParameters] = useImmer<ParameterWithValue[]>([]);

  useEffect(() => {
    invoke<string[]>("list_presets").then(setPresets);

    invoke<Parameter[]>("get_parameters").then(async (parameters) => {
      const promises = parameters.map(async (parameter) => {
        const value = await invoke<number>("get_live_parameter", { parameter: parameter.key });

        return {
          ...parameter,
          value
        }
      });

      setParameters(await Promise.all(promises))
    });

  }, []);

  useEffect(() => {
    for (const parameter of parameters) {
      invoke("set_live_param", { parameter: parameter.key, value: parameter.value })
    }
  }, [parameters])


  const parametersWithOnChange = parameters.map(parameter => ({
    ...parameter,

    onChange(newValue: number) {
      setParameters(parameters => {
        parameters.find(p => p.key == parameter.key)!.value = newValue;
      })
    }
  }))

  const options = presets.map((presetName: string) => ({
    value: presetName,
    label: capitalizeFirstLetter(presetName)
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

        <SliderCollection parameters={parametersWithOnChange} />
      </ContentBox>
    </Layout>
  );
}
