import { updateParameterValues } from "../commands";
import { useState } from "react";

interface ComponentProps {
  slider: String;
}
export default function ParameterSlider({ slider }: ComponentProps) {
  const [_slider, setSlider] = useState<Number>(1.0);

  return (
    <>
      <label htmlFor={"slider" + slider}>Slider {slider}</label>
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
