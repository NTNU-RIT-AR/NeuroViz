import { Parameter } from "../interfaces";
import Slider from "./Slider";

import styles from "./styles/SliderCollection.module.css";

export interface StateParameter extends Parameter {
  onChange: (value: number) => void
  value: number
}

interface SliderCollectionProps {
  parameters: StateParameter[]
}

export default function SliderCollection({ parameters }: SliderCollectionProps) {

  return (
    <div className={styles.sliderCollection}>
      {parameters.map((parameter) => (
        <Slider
          key={parameter.key}
          name={parameter.name}
          min={0}
          max={1}
          value={parameter.value}
          onChange={parameter.onChange}
        ></Slider>
      ))}
    </div>
  )
}
