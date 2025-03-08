import ParameterSlider from "../components/ParameterSlider";
import { useSliders } from "../SliderProviders";

export default function LiveViewPage() {
  const {
    slider1,
    slider2,
    slider3,
    slider4,
    setSlider1,
    setSlider2,
    setSlider3,
    setSlider4,
  } = useSliders();

  return (
    <>
      <h1>Live View</h1>
      <form className="parameters">
        <ParameterSlider number={"1"} slider={slider1} setSlider={setSlider1} />
        <ParameterSlider number={"2"} slider={slider2} setSlider={setSlider2} />
        <ParameterSlider number={"3"} slider={slider3} setSlider={setSlider3} />
        <ParameterSlider number={"4"} slider={slider4} setSlider={setSlider4} />
      </form>
    </>
  );
}
