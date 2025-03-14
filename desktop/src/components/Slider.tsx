import { useState, useEffect } from "react";
import { invoke } from "@tauri-apps/api/core";


type SliderProps = {
  name: string,
  min: number,
  max: number
}

export default function Slider({ name, min, max }: SliderProps) {
  const [value, setValue] = useState(0.0);

  useEffect(() => {
    try {
      invoke<number>("get_param", { paramName: name }).then(setValue);
    } catch (e) {
      console.log("get_param error: ", e);
    }
  }, []);

  const handleChange = (newValue: number) => {
    setValue(newValue);
    try {
      invoke("set_param", { paramName: name, value: newValue });
    } catch (e) {
      console.log("set_param error: ", e);
    }

  };

  return (
    <div>
      <input
        type="number"
        min={min}
        max={max}
        step={(max - min) / 100}
        value={value}
        onChange={(e) => handleChange(parseFloat(e.target.value))}
      />
      <input
        type="range"
        min={min}
        max={max}
        step={(max - min) / 100}
        value={value}
        onChange={(e) => handleChange(parseFloat(e.target.value))}
      />
      <p>Value: {value}</p>
    </div>
  );
}
