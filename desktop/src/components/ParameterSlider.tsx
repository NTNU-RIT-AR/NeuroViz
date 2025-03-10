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
      <div className={styles.subcontainer}>
        <label htmlFor={"slider" + number}>Slider {number}</label>
        <input
          type="number"
          step={0.01}
          min="0"
          max="1"
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
