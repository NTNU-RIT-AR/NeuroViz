import { useState } from "react";
import reactLogo from "./assets/react.svg";
import { invoke } from "@tauri-apps/api/core";
import "./App.css";

function App() {
  const [greetMsg, setGreetMsg] = useState("");
  const [name, setName] = useState("");
  const [slider1, setSlider1] = useState<Number>(1.0);
  const [slider2, setSlider2] = useState<Number>(1.0);
  const [slider3, setSlider3] = useState<Number>(1.0);
  const [slider4, setSlider4] = useState<Number>(1.0);

  async function greet() {
    // Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
    setGreetMsg(await invoke("greet", { name }));
  }

  async function updateParameterValues(
    s1: Number,
    s2: Number,
    s3: Number,
    s4: Number
  ) {
    await invoke("update_slider", {
      newSlider1: s1,
      newSlider2: s2,
      newSlider3: s3,
      newSlider4: s4,
    });
  }

  return (
    <main className="container">
      <h1>Welcome to Tauri + React</h1>

      <div className="row">
        <a href="https://vitejs.dev" target="_blank">
          <img src="/vite.svg" className="logo vite" alt="Vite logo" />
        </a>
        <a href="https://tauri.app" target="_blank">
          <img src="/tauri.svg" className="logo tauri" alt="Tauri logo" />
        </a>
        <a href="https://reactjs.org" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <p>Click on the Tauri, Vite, and React logos to learn more.</p>

      <form
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
      <p>{greetMsg}</p>

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
    </main>
  );
}

export default App;
