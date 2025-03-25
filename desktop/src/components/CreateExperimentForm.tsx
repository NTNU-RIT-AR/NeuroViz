import { useState } from "react";

export default function CreateExperimentForm() {
  const [experimentType, setExperimentType] = useState<string>("choice");

  return (
    <>
      <form>
        <h1>Create an Experiment</h1>
        {/* <label htmlFor="nickname">Nickname for experiment</label>
        <input
          name="nickname"
          type="text"
          placeholder="Spring 2025 experiment POCS"
        ></input>

        <label htmlFor="presets"></label>
        <input name="presets" type="checkbox">
          {/* TODO: Gjøre om til å være dynamisk og ikke hardkodet
          <option value="very-hue">Very hue</option>
          <option value="smoothish">Smoothish</option>
          <option value="high-emission">High emission</option>
          <option value="metal-looking">Metal looking</option>
        </input>

        <input
          onChange={(e) => {
            setExperimentType(e.currentTarget.value);
          }}
          value="choice"
          name="experiment_type"
          type="radio"
        >
          Forced choice
        </input>
        <input
          onChange={(e) => {
            setExperimentType(e.currentTarget.value);
          }}
          value="rating"
          name="experiment_type"
          type="radio"
        >
          1-5 star rating
        </input>

        {experimentType == "choice" && (
          <>
            <p>Choices here</p>
          </>
        )} */}
      </form>
    </>
  );
}
