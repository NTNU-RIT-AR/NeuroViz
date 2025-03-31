import Select from "react-select";
import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection";
import { useEffect, useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { capitalizeFirstLetter } from "../utils";

export default function LiveViewPage() {

  const [selectedPreset, setSelectedPreset] = useState<string | undefined>();

  const [presets, setPresets] = useState<string[]>([]);

  useEffect(() => {
    invoke<string[]>("list_presets").then(setPresets)

  }, []);

  const options = presets.map((presetName: string) => ({
    value: presetName,
    label: capitalizeFirstLetter(presetName)
  }
  ));


  return (
    <Layout title="Live View">
      <ContentBox>

        <Select options={options} onChange={(option) => setSelectedPreset(option?.value)} />

        <SliderCollection />
      </ContentBox>
    </Layout>
  );
}
