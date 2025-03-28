import { invoke } from "@tauri-apps/api/core";
import { useEffect, useState } from "react";

import styles from "./styles/ParameterSlider.module.css";

type SliderProps = {
  name: string;
  min: number;
  max: number;
};

// TODO make Slider dummy component?
export default function Slider({ name, min, max }: SliderProps) {
  const [value, setValue] = useState(0.0);

  useEffect(() => {
    try {
      invoke<number>("get_param", { parameter: name }).then(setValue);
    } catch (e) {
      console.log("get_param error: ", e);
    }
  }, []);

  const handleChange = (newValue: number) => {
    setValue(newValue);
    try {
      invoke("set_param", { parameter: name, value: newValue });
    } catch (e) {
      console.log("set_param error: ", e);
    }
  };

  return (
    <div className={styles.mainContainer}>
      <div className={styles.numberNameContainer}>
        <p>{name}</p>
        <input
          type="number"
          min={min}
          max={max}
          step={(max - min) / 100}
          value={value}
          onChange={(e) => {
            handleChange(
              Math.min(Math.max(parseFloat(e.target.value), min), max)
            );
          }}
        />
      </div>
      <div>
        <input
          type="range"
          className={styles.slider}
          min={min}
          max={max}
          step={(max - min) / 100}
          value={value}
          onChange={(e) => handleChange(parseFloat(e.target.value))}
        />
      </div>
      <div className={styles.minMaxContainer}>
        <p>{min}</p>
        <p>{max}</p>
      </div>
    </div>
  );
}
