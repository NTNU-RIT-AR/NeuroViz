import Slider from "./Slider";

export default function SliderCollection() {

  let params: string[] = ["Hue", "Smoothness", "Metallic", "Emission"];

  return (
    <div>
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
