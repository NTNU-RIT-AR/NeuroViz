import { useState } from "react";
import reactLogo from "./assets/react.svg";
import { invoke } from "@tauri-apps/api/core";
import "./App.css";

function App() {
  const [greetMsg, setGreetMsg] = useState("");
  const [name, setName] = useState("");
  const [slider1, setSlider1] = useState<Number>(0.0);
  const [slider2, setSlider2] = useState<Number>(0.0);
  const [slider3, setSlider3] = useState<Number>(0.0);
  const [slider4, setSlider4] = useState<Number>(0.0);

  async function greet() {
    // Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
    setGreetMsg(await invoke("greet", { name }));
  }

  async function updateParameterValues() {
    if (
      !(await invoke("update_slider", {
        newSlider1: slider1,
        newSlider2: slider2,
        newSlider3: slider3,
        newSlider4: slider4,
      }))
    ) {
      alert("Failed to update parameter values.");
    }
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

      <form>
        <input
          type="number"
          value={String(slider1)}
          onChange={(e) => {
            e.preventDefault();
            setSlider1(Number(e.currentTarget.value));
            updateParameterValues();
            // alert("Updated parameter values");
          }}
        ></input>
      </form>
    </main>
  );
}

export default App;
