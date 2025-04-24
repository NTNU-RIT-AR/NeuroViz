import { Parameter } from "../bindings.gen";
import Slider from "./Slider";

import styles from "./SliderCollection.module.css";

export interface StateParameter extends Parameter {
  onChange?: (value: number) => void;
  value: number;
}

interface SliderCollectionProps {
  parameters: StateParameter[];
}

export default function SliderCollection({
  parameters,
}: SliderCollectionProps) {
  return (
    <div className={styles.sliderCollection}>
      {parameters.map((parameter) => (
        <Slider
          key={parameter.key}
          name={parameter.name}
          min={parameter.min}
          max={parameter.max}
          value={parameter.value}
          onChange={parameter.onChange}
        ></Slider>
      ))}
    </div>
  );
}
