import { Parameters } from "../interfaces";
import ParameterSlider from "./ParameterSlider"

interface ParameterPresetProps {
    parameters: Parameters
}
export default function ParametersPreset({parameters}: ParameterPresetProps){
    return (
        <>
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
        </>
    )
}