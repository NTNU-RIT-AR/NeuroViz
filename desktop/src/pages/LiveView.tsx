import { useEffect, useState } from "react";
import ParameterSlider from "../components/ParameterSlider";
import Slider from "../components/Slider";
import { useSliders } from "../SliderProviders";

export default function LiveViewPage() {
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

  const [preset, setPreset] = useState("");

  useEffect(() => {

    return () => {

    };
  }, [preset]);

  return (
    <>
      <button onClick={() => setPreset("test1")}>SetPreset</button>
      <Slider
        name={"Hue"}
        min={0}
        max={1}
      ></Slider>

      <h1>Live View</h1>
      <form className="parameters">
        <ParameterSlider
          name={"Hue"}
          min={0}
          max={1}
          slider={hue}
          setSlider={setHue}
        />
        <ParameterSlider
          name={"Smoothness"}
          min={0}
          max={1}
          slider={smoothness}
          setSlider={setSmoothness}
        />
        <ParameterSlider
          name={"Metallic"}
          min={0}
          max={1}
          slider={metallic}
          setSlider={setMetallic}
        />
        <ParameterSlider
          name={"Emission"}
          min={0}
          max={1}
          slider={emission}
          setSlider={setEmission}
        />
      </form>
    </>
  );
}
