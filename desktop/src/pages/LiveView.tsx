import { useEffect, useState } from "react";
import SliderCollection from "../components/SliderCollection";

export default function LiveViewPage() {

  const [preset, setPreset] = useState("");

  useEffect(() => {

    return () => {

    };
  }, [preset]);

  return (
    <>
      <button onClick={() => setPreset("test1")}>SetPreset</button>
      <h1>Live View</h1>
      <SliderCollection />
    </>
  );
}
