import styles from "./styles/ParameterSlider.module.css";

type SliderProps = {
  name: string;
  min: number;
  max: number;
  value: number;
  onChange: (value: number) => void;
};

export default function Slider({
  name,
  min,
  max,
  value,
  onChange,
}: SliderProps) {
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
          onChange={(e) =>
            onChange(Math.min(Math.max(parseFloat(e.target.value), min), max))
          }
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
          onChange={(e) => onChange(parseFloat(e.target.value))}
        />
      </div>
      <div className={styles.minMaxContainer}>
        <p>{min}</p>
        <p>{max}</p>
      </div>
    </div>
  );
}
