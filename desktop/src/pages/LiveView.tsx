import ParameterSlider from "../components/ParameterSlider";
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

  return (
    <>
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
