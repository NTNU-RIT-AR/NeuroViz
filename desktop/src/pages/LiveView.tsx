import { useEffect, useState } from "react";
import SliderCollection from "../components/SliderCollection";

export default function LiveViewPage() {



  return (
    <>
      <h1>Live View</h1>
      <div className="contentBox">
        <SliderCollection />
      </div>
    </>
  );
}
