import { useState } from "react";
import { match } from "ts-pattern";
import { commands, CurrentPreset, ExperimentState } from "../../bindings.gen";
import Button from "../../components/Button";
import Slider from "../../components/Slider";
import styles from "./ActiveExperiment.module.css";
import { PresetBox } from "./PresetBox";

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

  const actions = match(experimentState)
    .with({ experiment_type: "choice" }, (experiment_state) => {
      let { experiment, current_preset } = experiment_state;
      const choice = experiment.choices[experimentState.current_index];

      const presetA = presets[choice.a]!;
      const presetB = presets[choice.b]!;

      const selectedPreset = match(current_preset)
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
    .with({ experiment_type: "rating" }, (experiment_state) => {
      const { experiment } = experiment_state;
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

  const experimentType = match(experimentState.experiment_type)
    .with("choice", () => "Choice")
    .with("rating", () => "Rating")
    .exhaustive();

  function onSubmit() {
    match(experimentState)
      .with({ experiment_type: "choice" }, () => {
        commands.answerExperiment({
          experiment_type: "choice",
        });
      })
      .with({ experiment_type: "rating" }, () => {
        commands.answerExperiment({
          experiment_type: "rating",
          value: sliderValue,
        });
      })
      .exhaustive();
  }

  const questionIndex = experimentState.current_index + 1;

  const questionAmount = match(experimentState)
    .with(
      { experiment_type: "choice" },
      (state) => state.experiment.choices.length
    )
    .with(
      { experiment_type: "rating" },
      (state) => state.experiment.order.length
    )
    .exhaustive();

  const confirmText = match(experimentState)
    .with(
      { experiment_type: "choice" },
      (state) => `Choose ${state.current_preset}`
    )
    .with({ experiment_type: "rating" }, () => "Submit rating")
    .exhaustive();

  const progessBarWidth = `${(questionIndex / questionAmount) * 100}%`;

  const content = match(experimentState)
    .with({ experiment_type: "choice" }, (state) => {
      const choice = state.experiment.choices[state.current_index];

      function presetBoxFor(presetKey: string, type: CurrentPreset) {
        const isSelected = type === state.current_preset;
        const preset = state.experiment.presets[presetKey]!;

        function onClick() {
          if (isSelected) {
            return;
          }

          commands.swapPreset();
        }

        return (
          <PresetBox
            title={`Preset ${type}`}
            preset={preset}
            toggleable
            active={isSelected}
            onClick={onClick}
          />
        );
      }

      return (
        <main className={styles.choiceContent}>
          {presetBoxFor(choice.a, "A")}

          <span className={styles.vs}>vs</span>

          {presetBoxFor(choice.b, "B")}
        </main>
      );
    })
    .with({ experiment_type: "rating" }, (state) => {
      const preset_key = state.experiment.order[state.current_index];
      const preset = state.experiment.presets[preset_key]!;

      return (
        <main className={styles.ratingContent}>
          <PresetBox title="Preset" preset={preset} active />

          <Slider
            name="Rating"
            min={1}
            max={5}
            step={1}
            value={sliderValue}
            onChange={setSldierValue}
          />
        </main>
      );
    })
    .exhaustive();

  return (
    <div>
      <div
        className={styles.progressBar}
        style={{ width: progessBarWidth }}
      ></div>

      <div className={styles.layout}>
        <header className={styles.header}>
          <div>
            <h1 className={styles.title}>Experiment</h1>

            <h2 className={styles.subtitle}>
              {experimentState.experiment.name}
            </h2>
          </div>

          <div>
            <h1 className={styles.title}>Question</h1>

            <h2 className={styles.subtitle}>
              {questionIndex}/{questionAmount}
            </h2>
          </div>
        </header>

        {content}

        <footer className={styles.footer}>
          <Button variant="danger" onClick={handleExit}>
            Exit
          </Button>
          {/* <Button variant="primary">Choose B</Button> */}
          <Button variant="primary" onClick={onSubmit}>
            {confirmText}
          </Button>
        </footer>
      </div>
    </div>
  );
}
