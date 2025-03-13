import { useState, useEffect } from "react";
import { invoke } from "@tauri-apps/api/core";
import { emit } from "@tauri-apps/api/event";


type SliderProps = {
  name: string,
  min: number,
  max: number
}

export default function Slider({ name, min, max }: SliderProps) {
  const [value, setValue] = useState(0.0);

  useEffect(() => {
    invoke<number>("get_param", { param_name: name }).then(setValue);
  }, []);

  const handleChange = (newValue: number) => {
    setValue(newValue);
    invoke("set_param", { param_name: name, value: newValue });
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
