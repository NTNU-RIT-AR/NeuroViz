import { useRef, useState } from "react";
import { Layout } from "../components/Layout";

import { EyeIcon, PlayIcon, TrashIcon } from "@heroicons/react/24/outline";
import { commands, Experiment, WithKey } from "../bindings.gen";
import Button from "../components/Button";
import Popup from "../components/Popup";
import { useCommand } from "../hooks";
import styles from "./styles/Experiments.module.css";

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

  const [show, setShow] = useState(false);
  const [showCreatePopup, setShowCreatePopup] = useState(false);

  const resultNameRef = useRef<HTMLInputElement>(null);
  const observerIdRef = useRef<HTMLInputElement>(null);
  const noteRef = useRef<HTMLTextAreaElement>(null);

  return (
    <>
      <div className={styles.experimentCard}>
        <h3 className={styles.experimentCardTitle}>{experiment.value.name}</h3>
        <div className={styles.experimentCardContent}>
          <p>Type: {experiment.value.experiment_type}</p>
        </div>
        <div className={styles.experimentCardBottom}>
          {/* Delete button */}
          <Button square={true} onClick={onDelete}>
            <TrashIcon className="icon" />
          </Button>

          {/* Open button */}
          <Button square={true} onClick={() => setShow(true)}>
            <EyeIcon className="icon" />
          </Button>

          {/* Start button */}
          <Button square={true} onClick={() => setShowCreatePopup(true)}>
            <PlayIcon className="icon" />
          </Button>
        </div>
      </div>

      {show && <ViewExperiment data={experiment.value} setShow={setShow} />}

      {showCreatePopup && (
        <Popup
          title="Start experiment"
          onClose={() => setShowCreatePopup(false)}
        >
          <div className={styles.inputFields}>
            <label className={styles.inputGroup}>
              Experiment result name
              <input ref={resultNameRef} className={styles.input} type="text" />
            </label>

            <label className={styles.inputGroup}>
              Observer ID
              <input
                ref={observerIdRef}
                className={styles.input}
                type="number"
              />
            </label>

            <label className={styles.inputGroup}>
              Note
              <textarea ref={noteRef} className={styles.input} />
            </label>
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

export default function ExperimentsPage() {
  const experiments = useCommand(commands.getExperiments);

  return (
    <>
      <Layout
        title="Experiments"
        scrollable={true}
        toolbar={<Button onClick={() => {}}>Create Experiment</Button>}
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
    </>
  );
}
