import { updateParameterValues } from "../commands";

interface ComponentProps {
  setSlider: (n: Number) => void;
  slider: Number;
}
export default function ParameterSlider({ setSlider, slider }: ComponentProps) {
  return (
    <>
      <label htmlFor={"slider" + slider}>Slider {slider.toString()}</label>
      <input
        name={"slider" + slider}
        type="range"
        min="0"
        step={0.01}
        max="1"
        onChange={(e) => {
          e.preventDefault();
          updateParameterValues(slider, Number(e.currentTarget.value));
          setSlider(parseFloat(e.currentTarget.value));
        }}
      ></input>
    </>
  );
}
