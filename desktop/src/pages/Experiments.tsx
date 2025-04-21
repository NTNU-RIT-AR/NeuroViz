import { useRef, useState } from "react";
import { Layout } from "../components/Layout";

import { PlayIcon, TrashIcon } from "@heroicons/react/24/outline";
import Select from "react-select";
import { match } from "ts-pattern";
import { commands, Experiment, Preset, WithKey } from "../bindings.gen";
import Button from "../components/Button";
import {
  Label,
  NumberInput,
  TextArea,
  TextInput,
} from "../components/InputField";
import Popup from "../components/Popup";
import { useCommand } from "../hooks";
import styles from "./Experiments.module.css";

enum FilterState {
  All = "All",
  Published = "Published",
  Draft = "Draft",
}

interface ViewExperimentProps {
  data: Experiment;
  setShow: React.Dispatch<React.SetStateAction<boolean>>;
}

function ViewExperiment({ data, setShow }: ViewExperimentProps) {
  let close = () => {
    setShow(false);
  };
  return (
    <Popup title={data.name} onClose={close}>
      <h3>{data.name}</h3>
    </Popup>
  );
}

interface ExperimentCardProps {
  experiment: WithKey<Experiment>;
  onDelete: () => void;
  onStart: (resultName: string, observerId: number, note: string) => void;
}

function ExperimentCard(props: ExperimentCardProps) {
  const { experiment, onDelete, onStart } = props;

  const [showCreatePopup, setShowCreatePopup] = useState(false);

  const resultNameRef = useRef<HTMLInputElement>(null);
  const observerIdRef = useRef<HTMLInputElement>(null);
  const noteRef = useRef<HTMLTextAreaElement>(null);

  const experimentType = match(experiment.value.experiment_type)
    .with("rating", () => "Rating")
    .with("choice", () => "Choice")
    .exhaustive();

  const questionsAmount = match(experiment.value)
    .with(
      { experiment_type: "rating" },
      (experiment) => experiment.order.length
    )
    .with(
      { experiment_type: "choice" },
      (experiment) => experiment.choices.length
    )
    .exhaustive();

  return (
    <>
      <div className={styles.experimentCard}>
        <h3 className={styles.experimentCardTitle}>{experiment.value.name}</h3>
        <div className={styles.experimentCardContent}>
          <p>Type: {experimentType}</p>
          <p>Questions: {questionsAmount}</p>
        </div>
        <div className={styles.experimentCardBottom}>
          {/* Delete button */}
          <Button
            variant="danger"
            square={true}
            onClick={() => {
              if (confirm("Are you sure you want to delete this experiment?")) {
                onDelete();
              }
            }}
          >
            <TrashIcon className="icon" />
          </Button>
          {/* Start button */}
          <Button
            variant="primary"
            square={true}
            onClick={() => setShowCreatePopup(true)}
          >
            <PlayIcon className="icon" />
          </Button>
        </div>
      </div>

      {showCreatePopup && (
        <Popup
          title="Start experiment"
          onClose={() => setShowCreatePopup(false)}
        >
          <div className={styles.inputFields}>
            <Label>
              Experiment result name
              <TextInput ref={resultNameRef} />
            </Label>

            <Label>
              Observer ID
              <NumberInput ref={observerIdRef} />
            </Label>

            <Label>
              Note
              <TextArea ref={noteRef} />
            </Label>
          </div>

          <Button
            variant="primary"
            onClick={() => {
              const resultName = resultNameRef.current?.value;
              const observerId = observerIdRef.current?.value;
              const note = noteRef.current?.value;

              if (resultName && observerId) {
                onStart(resultName, parseInt(observerId), note || "");
                setShowCreatePopup(false);
              }
            }}
          >
            Start
          </Button>
        </Popup>
      )}
    </>
  );
}

function CreateChoiceExperiment() {
  return <div>Create Choice Experiment</div>;
}

interface CreateExperimentPopupProps {
  presets: WithKey<Preset>[];
  onClose: () => void;
}

const options = [
  {
    value: "choice",
    label: "Choice",
  } as const,
  {
    value: "rating",
    label: "Rating",
  } as const,
];

// Choice: select two
// Create all combinations of presets? can select

function CreateExperimentPopup(props: CreateExperimentPopupProps) {
  const { onClose, presets } = props;
  const [experimentType, setExperimentType] = useState(options[0].value);

  return (
    <Popup title="Create Experiment" onClose={onClose}>
      <Select options={options} defaultValue={options[0]} />

      {/* <Select
        name="type"
        aria-label="Experiment type"
        value={experimentType}
        onChange={(e) => setExperimentType(e.currentTarget.value as any)}
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.name}
          </option>
        ))}
      </Select> */}

      <Select
        // defaultValue={[colourOptions[2], colourOptions[3]]}
        isMulti
        name="presets"
        options={
          presets.map((preset) => ({
            value: preset.key,
            label: preset.value.name,
          })) as any
        }
        className="basic-multi-select"
        classNamePrefix="select"
      />
    </Popup>
  );
}

export default function ExperimentsPage() {
  const experiments = useCommand(commands.getExperiments);
  const presets = useCommand(commands.getPresets).data;
  const [showCreatePopup, setShowCreatePopup] = useState(false);

  return (
    <>
      <Layout
        title="Experiments"
        folder="Experiments"
        scrollable={true}
        toolbar={
          <Button onClick={() => setShowCreatePopup(true)}>
            Create Experiment
          </Button>
        }
      >
        <div className={styles.experimentsContainer}>
          {experiments.data.map((experiment) => (
            <ExperimentCard
              key={experiment.key}
              experiment={experiment}
              onStart={(resultName, observerId, note) =>
                commands.startExperiment(
                  experiment.key,
                  resultName,
                  observerId,
                  note
                )
              }
              onDelete={async () => {
                await commands.deleteExperiment(experiment.key);
                experiments.refetch();
              }}
            />
          ))}
        </div>
      </Layout>

      {showCreatePopup && (
        <CreateExperimentPopup
          onClose={() => setShowCreatePopup(false)}
          presets={presets}
        />
      )}
    </>
  );
}
