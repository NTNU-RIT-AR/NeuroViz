import { useRef, useState } from "react";
import { Layout } from "../components/Layout";

import { PlayIcon, TrashIcon } from "@heroicons/react/24/outline";
import { SelectInstance } from "react-select";
import { match } from "ts-pattern";
import { commands, Experiment, Preset, WithKey } from "../bindings.gen";
import Button from "../components/Button";
import { Input, Label, Select, TextArea } from "../components/Input";
import Popup from "../components/Popup";
import { useCommand } from "../hooks";
import styles from "./Experiments.module.css";
import { useMemo } from "react";
import Fuse, { FuseResult } from "fuse.js";



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
              <Input ref={resultNameRef} />
            </Label>

            <Label>
              Observer ID
              <Input ref={observerIdRef} type="number" />
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

interface CreateExperimentPopupProps {
  presets: WithKey<Preset>[];
  onClose: () => void;
}

interface Option {
  value: string;
  label: string;
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

function CreateExperimentPopup(props: CreateExperimentPopupProps) {
  const { onClose, presets } = props;
  const [experimentType, setExperimentType] = useState(options[0]);

  const experimentNameRef = useRef<HTMLInputElement>(null);
  const presetsRef = useRef<SelectInstance<Option, true>>(null);

  function createExperiment() {
    const experimentName = experimentNameRef.current!.value;

    const selectedPresets = presetsRef
      .current!.getValue()
      .map((preset) => preset.value);

    if (!experimentName) {
      alert("Experiment name is required");
      return;
    }

    if (selectedPresets.length < 2) {
      alert("At least two presets are required");
      return;
    }

    function onSuccess(path: string) {
      alert(`Experiment created successfully! Saved to ${path}`);
      onClose();
    }

    match(experimentType.value)
      .with("choice", () => {
        const combinations = selectedPresets.flatMap((presetA) =>
          selectedPresets
            .filter((presetB) => presetA !== presetB)
            .map((presetB) => ({
              a: presetA,
              b: presetB,
            }))
        );

        commands
          .createExperiment({
            experiment_type: "choice",
            name: experimentName,
            presets: selectedPresets,
            choices: combinations,
          })
          .then(onSuccess)
          .catch(alert);
      })
      .with("rating", () => {
        commands
          .createExperiment({
            experiment_type: "rating",
            name: experimentName,
            presets: selectedPresets,
            order: selectedPresets,
          })
          .then(onSuccess)
          .catch(alert);
      })
      .exhaustive();
  }

  return (
    <Popup title="Create Experiment" onClose={onClose}>
      <div className={styles.inputFields}>
        <Label>
          Experiment type
          <Select
            options={options}
            value={experimentType}
            onChange={(value) => setExperimentType(value!)}
          />
        </Label>

        <Label>
          Experiment name
          <Input ref={experimentNameRef} />
        </Label>

        <Label>
          Presets
          <Select
            ref={presetsRef}
            isMulti
            name="presets"
            options={presets.map((preset) => ({
              value: preset.key,
              label: preset.value.name,
            }))}
            className="basic-multi-select"
            classNamePrefix="select"
          />
          {experimentType.value === "choice" && (
            <p className={styles.infoText}>
              Experiment contain all combinations of presets
            </p>
          )}
        </Label>

        <Button
          variant="primary"
          onClick={createExperiment}
          className={styles.createButton}
        >
          Create
        </Button>
      </div>
    </Popup>
  );
}

export default function ExperimentsPage() {
  const experiments = useCommand(commands.getExperiments);
  const presets = useCommand(commands.getPresets).data;
  const [showCreatePopup, setShowCreatePopup] = useState(false);

  const [search, setSearch] = useState("");
  const fuse = useMemo(() => new Fuse(experiments.data, {
    keys: ["value.name"],
    threshold: 0.3,
  }), [experiments.data]);
  const filteredExperiments = useMemo(() => {
    if (!search.trim()) return experiments.data;
    return fuse.search(search).map((res: FuseResult<WithKey<Experiment>>) => res.item)

  }, [search, fuse]);

  return (
    <>
      <Layout
        title="Experiments"
        folder="Experiments"

        toolbar={
          <div className={styles.headerRow}>
            <div className={styles.searchBoxWrapper}>
              <Input
                placeholder="Search experiments"
                value={search}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSearch(e.target.value)}
              />
            </div>
            <Button onClick={() => setShowCreatePopup(true)}>
              Create Experiment
            </Button>
          </div>
        }
      >
        <div className={styles.experimentsContainer}>
          {filteredExperiments.map((experiment) => (
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
          onClose={() => {
            experiments.refetch();
            setShowCreatePopup(false);
          }}
          presets={presets}
        />
      )}
    </>
  );
}
