import Slider from "./Slider";

import styles from "./styles/SliderCollection.module.css";

export default function SliderCollection() {

  let params: string[] = ["Hue", "Smoothness", "Metallic", "Emission"];

  return (
    <div className={styles.sliderCollection}>
      {params.map((name) => (
        <Slider
          name={name}
          min={0}
          max={1}
        ></Slider>
      ))}
    </div>
  )
}
