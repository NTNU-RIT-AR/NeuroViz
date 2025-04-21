import classNames from "classnames";
import styles from "./styles/ParameterSlider.module.css";

type SliderProps = {
  name: string;
  min: number;
  max: number;
  step?: number;
  value: number;
  onChange?: (value: number) => void;
};

export default function Slider(props: SliderProps) {
  const { name, min, max, value, onChange } = props;

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    if (onChange) {
      const newValue = parseFloat(e.target.value);
      onChange(Math.min(Math.max(newValue, min), max));
    }
  }

  const step = props.step || (max - min) / 100;
  const isReadonly = onChange === undefined;

  return (
    <div
      className={classNames(
        styles.mainContainer,
        isReadonly && styles.readonly
      )}
      style={
        {
          "--min": min,
          "--max": max,
          "--value": value,
        } as React.CSSProperties
      }
    >
      <div className={styles.numberNameContainer}>
        <p>{name}</p>
        <input
          type="number"
          min={min}
          max={max}
          value={value}
          onChange={handleChange}
        />
      </div>
      <div>
        <input
          type="range"
          className={styles.slider}
          min={min}
          max={max}
          step={step}
          value={value}
          onChange={handleChange}
        />
      </div>
      <div className={styles.minMaxContainer}>
        <p>{min}</p>
        <p>{max}</p>
      </div>
    </div>
  );
}
