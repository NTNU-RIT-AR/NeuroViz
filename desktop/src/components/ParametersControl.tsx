import { Parameters } from "../interfaces";
import { useSliders } from "../SliderProviders";
import ParameterSlider from "./ParameterSlider"

interface ParameterControlProps {
  parameters: Parameters,
  presetName: string
}
export default function ParameterControl({ parameters }: ParameterControlProps) {

  const {
    hue,
    smoothness,
    metallic,
    emission,
    setHue,
    setSmoothness,
    setMetallic,
    setEmission,
  } = useSliders();

  return (
    <form className="parameters">
      <ParameterSlider
        name={"Hue"}
        min={0}
        max={1}
        slider={parameters.hue}
        readOnly={true}
        setSlider={undefined}
      />
      <ParameterSlider
        name={"Smoothness"}
        min={0}
        max={1}
        slider={parameters.smoothness}
        readOnly={true}
        setSlider={undefined}
      />
      <ParameterSlider
        name={"Metallic"}
        min={0}
        max={1}
        slider={parameters.metallic}
        readOnly={true}
        setSlider={undefined}
      />
      <ParameterSlider
        name={"Emission"}
        min={0}
        max={1}
        slider={parameters.emission}
        readOnly={true}
        setSlider={undefined}
      />
    </form>
  )
}
