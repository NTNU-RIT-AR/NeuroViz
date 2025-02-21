import ParameterSlider from "./components/ParameterSlider";

import QRcode from "./components/QRcode";
import "./App.css";

function App() {
  return (
    <main className="container">
      <h1>Connect on the app</h1>
      <QRcode />
      <form className="parameters">
        <ParameterSlider slider={"1"} />
        <ParameterSlider slider={"2"} />
        <ParameterSlider slider={"3"} />
        <ParameterSlider slider={"4"} />
      </form>
    </main>
  );
}

export default App;
