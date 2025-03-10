import { updateParameterValues } from "../commands";

import styles from "./styles/ParameterSlider.module.css";

interface ComponentProps {
  name: string;
  min: number;
  max: number;
  slider: number;
  setSlider: (number: number) => void;
}
export default function ParameterSlider({
  name,
  min,
  max,
  slider,
  setSlider,
}: ComponentProps) {
  return (
    <div className={styles.container}>
      <div className={styles.subcontainer}>
        <label htmlFor={name}>{name}</label>
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
        name={name}
        className={styles.slider}
        type="range"
        step={(max - min) / 100}
        min={min}
        max={max}
        onChange={(e) => {
          e.preventDefault();
          updateParameterValues(name, Number(e.currentTarget.value));
          setSlider(parseFloat(e.currentTarget.value));
        }}
      ></input>
    </div>
  );
}
