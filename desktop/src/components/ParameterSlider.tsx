import { updateParameterValues } from "../commands";

import styles from "./styles/ParameterSlider.module.css";

interface ComponentProps {
  number: String;
  min: number;
  max: number;
  slider: number;
  setSlider: (number: number) => void;
}
export default function ParameterSlider({
  number,
  min,
  max,
  slider,
  setSlider,
}: ComponentProps) {
  return (
    <div className={styles.container}>
      <div className={styles.subcontainer}>
        <label htmlFor={"slider" + number}>Slider {number}</label>
        <input
          type="number"
          step={(max - min) / 100}
          min={min}
          max={max}
          value={slider}
          onChange={(event) => {
            setSlider(parseFloat(event.currentTarget.value));
          }}
        ></input>
      </div>
      <input
        value={slider}
        name={"slider" + number}
        className={styles.slider}
        type="range"
        step={(max - min) / 100}
        min={min}
        max={max}
        onChange={(e) => {
          e.preventDefault();
          updateParameterValues(number, Number(e.currentTarget.value));
          setSlider(parseFloat(e.currentTarget.value));
        }}
      ></input>
    </div>
  );
}
