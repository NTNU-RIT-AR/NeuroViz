import { useState } from "react";
import { updateParameterValues } from "./commands";
import ParameterSlider from "./components/ParameterSlider";

import QRcode from "./components/QRcode";
import "./App.css";

function App() {
  const [slider1, setSlider1] = useState<Number>(1.0);
  const [slider2, setSlider2] = useState<Number>(1.0);
  const [slider3, setSlider3] = useState<Number>(1.0);
  const [slider4, setSlider4] = useState<Number>(1.0);

  return (
    <main className="container">
      <h1>Connect on the app</h1>
      <QRcode />

      <form className="parameters">
        <ParameterSlider setSlider={setSlider1} slider={1} />
        <ParameterSlider setSlider={setSlider2} slider={2} />
        <ParameterSlider setSlider={setSlider3} slider={3} />
        <ParameterSlider setSlider={setSlider4} slider={4} />
      </form>
    </main>
  );
}

export default App;
