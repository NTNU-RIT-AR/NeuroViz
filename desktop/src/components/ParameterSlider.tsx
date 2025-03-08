import { updateParameterValues } from "../commands";

import styles from "./styles/ParameterSlider.module.css";

interface ComponentProps {
  number: String;
  slider: number;
  setSlider: (number: number) => void;
}
export default function ParameterSlider({
  number,
  slider,
  setSlider,
}: ComponentProps) {
  return (
    <div className={styles.container}>
      <label htmlFor={"slider" + number}>
        Slider {number} {slider.toString()}
      </label>
      <input
        value={slider}
        name={"slider" + number}
        className={styles.slider}
        type="range"
        min="0"
        step={0.01}
        max="1"
        onChange={(e) => {
          e.preventDefault();
          updateParameterValues(number, Number(e.currentTarget.value));
          setSlider(parseFloat(e.currentTarget.value));
        }}
      ></input>
    </div>
  );
}
