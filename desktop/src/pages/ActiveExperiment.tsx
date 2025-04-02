import { useState } from "react";
import { match } from "ts-pattern";
import { commands, ExperimentState } from "../bindings.gen";
import Button from "../components/Button";
import { Layout } from "../components/Layout";
import styles from "./styles/ActiveExperiment.module.css";

interface ActiveExperimentProps {
  experimentState: ExperimentState;
}

function handleExit() {
  const confirmExit = confirm("Are you sure you want to exit the experiment?");

  if (confirmExit) {
    // Call the command to exit the experiment
    commands.exitExperiment();
  }
}

export function ActiveExperiment(props: ActiveExperimentProps) {
  const { experimentState } = props;
  const { presets } = experimentState.experiment;
  const prompt = experimentState.current_index;

  const [sliderValue, setSldierValue] = useState(1);

  const actions = match(experimentState.experiment)
    .with({ experiment_type: "choice" }, (experiment) => {
      const choice = experiment.choices[experimentState.current_index];

      const presetA = presets[choice.a]!;
      const presetB = presets[choice.b]!;

      const current = experimentState.choice_current_preset;
      const selectedPreset = match(current)
        .with("A", () => presetA)
        .with("B", () => presetB)
        .exhaustive();

      return (
        <div>
          <p>Currently showing preset "{selectedPreset.name}"</p>

          <div style={{ display: "flex", gap: 16 }}>
            <Button onClick={() => commands.swapPreset()}>Swap</Button>
            <Button
              onClick={() =>
                commands.answerExperiment({ experiment_type: "choice" })
              }
            >
              Choose {selectedPreset.name}
            </Button>
          </div>
        </div>
      );
    })
    .with({ experiment_type: "rating" }, (experiment) => {
      const presetKey = experiment.order[experimentState.current_index];

      const preset = presets[presetKey]!;

      // TODO star rating

      return (
        <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          <p>Currently showing preset "{preset.name}"</p>

          <div style={{ display: "flex", gap: 16, paddingBottom: 8 }}>
            <input
              type="range"
              min={1}
              max={5}
              step={1}
              value={sliderValue}
              onChange={(e) => setSldierValue(parseInt(e.currentTarget.value))}
            />

            <span>{sliderValue}</span>
          </div>

          <Button
            onClick={() =>
              commands.answerExperiment({
                experiment_type: "rating",
                value: sliderValue,
              })
            }
          >
            Submit rating
          </Button>
        </div>
      );
    })
    .exhaustive();

  const experimentType = match(experimentState.experiment.experiment_type)
    .with("choice", () => "Choice")
    .with("rating", () => "Rating")
    .exhaustive();

  return (
    <Layout
      title={`${experimentType} experiment: ${experimentState.experiment.name}`}
      scrollable
      className={styles.container}
    >
      <div
        style={{
          flex: 1,
          flexDirection: "row",
          alignItems: "center",
          paddingBottom: 16,
        }}
      >
        <Button variant="danger" onClick={handleExit}>
          Exit
        </Button>
        <h1 style={{ display: "inline-block", paddingLeft: 16, lineHeight: 1 }}>
          Question {experimentState.current_index + 1}
        </h1>
      </div>

      {actions}

      {/* <Button>

      </Button> */}

      {/* <div>
        <p>Name: {experimentState.experiment.name}</p>
        <p>Type: {experimentState.experiment.experiment_type}</p>
      </div> */}
    </Layout>
  );
}
