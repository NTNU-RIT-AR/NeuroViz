import { useState } from "react";
import { match } from "ts-pattern";
import { commands, CurrentPreset, ExperimentState } from "../../bindings.gen";
import Button from "../../components/Button";
import ConnectionBox from "../../components/ConnectionBox";
import Popup from "../../components/Popup";
import Slider from "../../components/Slider";
import { useConnectionQrCode } from "../../hooks";
import styles from "./ActiveExperiment.module.css";
import { PresetBox } from "./PresetBox";

interface ActiveExperimentProps {
  experimentState: ExperimentState;
  isConnected: boolean;
}

function handleExit() {
  const confirmExit = confirm("Are you sure you want to exit the experiment?");

  if (confirmExit) {
    // Call the command to exit the experiment
    commands.exitExperiment();
  }
}

export default function ActiveExperiment(props: ActiveExperimentProps) {
  const { experimentState, isConnected } = props;

  const connectionQrCode = useConnectionQrCode();
  const [sliderValue, setSldierValue] = useState(1);

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

  const isDisabled = experimentState.is_idle;

  return (
    <>
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
            <Button variant="danger" onClick={handleExit} disabled={isDisabled}>
              Exit
            </Button>
            {/* <Button variant="primary">Choose B</Button> */}
            <Button variant="primary" onClick={onSubmit} disabled={isDisabled}>
              {confirmText}
            </Button>
          </footer>
        </div>
      </div>

      {!isConnected && (
        <Popup title={"Reconnect"}>
          <ConnectionBox qrText={connectionQrCode} isConnected={isConnected} />
        </Popup>
      )}
    </>
  );
}
