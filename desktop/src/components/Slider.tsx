import { useEffect, useState } from "react";

import { commands } from "../bindings.gen";
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
    // TODO remove as any
    commands.getParam(name as any).then((response) => {
      if (response.status === "ok") {
        setValue(response.data);
      } else {
        console.error("Error fetching parameter value: ", response.error);
      }
    });
  }, []);

  const handleChange = (newValue: number) => {
    setValue(newValue);

    // TODO remove as any
    commands.setParam(name as any, newValue);
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
