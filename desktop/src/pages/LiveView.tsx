import { useLocation } from "react-router-dom";
import ParameterSlider from "../components/ParameterSlider";

export default function LiveViewPage() {
  return (
    <>
      <h1>Live View</h1>
      <form className="parameters">
        <ParameterSlider slider={"1"} />
        <ParameterSlider slider={"2"} />
        <ParameterSlider slider={"3"} />
        <ParameterSlider slider={"4"} />
      </form>
    </>
  )
}
