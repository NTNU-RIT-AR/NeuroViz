import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { updateParameterValues } from "./commands";

import QRcode from "./components/QRcode";
import "./App.css";

function App() {
  // const [greetMsg, setGreetMsg] = useState("");
  // const [name, setName] = useState("");
  const [slider1, setSlider1] = useState<Number>(1.0);
  const [slider2, setSlider2] = useState<Number>(1.0);
  const [slider3, setSlider3] = useState<Number>(1.0);
  const [slider4, setSlider4] = useState<Number>(1.0);

  // async function greet() {
  //   // Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
  //   setGreetMsg(await invoke("greet", { name }));
  // }

  return (
    <main className="container">
      <h1>Connect on the app</h1>
      <QRcode />

      <form className="parameters">
        <label htmlFor="slider1">Slider 1</label>
        <input
          name="slider1"
          type="range"
          min="0"
          step={0.01}
          max="1"
          onChange={(e) => {
            e.preventDefault();
            updateParameterValues(
              Number(e.currentTarget.value),
              slider2,
              slider3,
              slider4
            );
            setSlider1(parseFloat(e.currentTarget.value));
          }}
        ></input>
        <label htmlFor="slider2">Slider 2</label>
        <input
          name="slider2"
          type="range"
          min="0"
          step={0.01}
          max="1"
          onChange={(e) => {
            e.preventDefault();
            updateParameterValues(
              slider1,
              Number(e.currentTarget.value),
              slider3,
              slider4
            );
            setSlider2(parseFloat(e.currentTarget.value));
          }}
        ></input>
        <label htmlFor="slider3">Slider 3</label>
        <input
          name="slider3"
          type="range"
          min="0"
          step={0.01}
          max="1"
          onChange={(e) => {
            e.preventDefault();
            updateParameterValues(
              slider1,
              slider2,
              Number(e.currentTarget.value),
              slider4
            );
            setSlider3(parseFloat(e.currentTarget.value));
          }}
        ></input>

        <label htmlFor="slider4">Slider 4</label>
        <input
          name="slider4"
          type="range"
          min="0"
          step={0.01}
          max="1"
          onChange={(e) => {
            e.preventDefault();
            updateParameterValues(
              slider1,
              slider2,
              slider3,
              Number(e.currentTarget.value)
            );
            setSlider4(parseFloat(e.currentTarget.value));
          }}
        ></input>
      </form>

      {/* <form
        className="row"
        onSubmit={(e) => {
          e.preventDefault();
          greet();
        }}
      >
        <input
          id="greet-input"
          onChange={(e) => setName(e.currentTarget.value)}
          placeholder="Enter a name..."
        />
        <button type="submit">Greet</button>
      </form>
      <p>{greetMsg}</p> */}
    </main>
  );
}

export default App;
