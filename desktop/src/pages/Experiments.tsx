import { useEffect, useState } from "react";
import { Layout } from "../components/Layout";

import { EyeIcon, PlayIcon, TrashIcon } from "@heroicons/react/24/outline";
import { commands, Experiment, WithKey } from "../bindings.gen";
import Button from "../components/Button";
import Popup from "../components/Popup";
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
}

function ExperimentCard({ experiment: experiment }: ExperimentCardProps) {
  const [show, setShow] = useState(false);
  return (
    <>
      <div className={styles.experimentCard}>
        <h3 className={styles.experimentCardTitle}>{experiment.value.name}</h3>
        <div className={styles.experimentCardContent}>
          <p>Type: {experiment.value.experiment_type}</p>
        </div>
        <div className={styles.experimentCardBottom}>
          <Button square={true}>
            <TrashIcon className="icon" />
          </Button>

          <Button square={true} onClick={() => setShow(true)}>
            <EyeIcon className="icon" />
          </Button>

          <Button
            square={true}
            onClick={() => {
              // TODO: Result name, observer id and note
              commands.startExperiment(experiment.key, "TODO name", 0, "note!");
            }}
          >
            <PlayIcon className="icon" />
          </Button>
        </div>
      </div>

      {show && <ViewExperiment data={experiment.value} setShow={setShow} />}
    </>
  );
}

export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<WithKey<Experiment>[]>([]);
  const [filter, setFilter] = useState<FilterState>(FilterState.All);

  useEffect(() => {
    commands.getExperiments().then(setExperiments);
  }, []);

  return (
    <>
      <Layout
        title="Experiments"
        scrollable={true}
        toolbar={<Button onClick={() => {}}>Create Experiment</Button>}
      >
        <div className={styles.experimentsContainer}>
          {experiments
            // .filter(
            //   (experiment) => (
            //     experiment
            //   )
            // )
            .map((experiment) => (
              <ExperimentCard experiment={experiment} />
            ))}
        </div>
      </Layout>

      {/* {viewCreateExperiment && <CreateExperimentForm />} */}
      {/* {viewCreateExperiment ? ( */}
      {/*   <Button */}
      {/*     theme={"neutral"} */}
      {/*     textContent="Cancel" */}
      {/*     onClick={toggleViewCreateExperiment} */}
      {/*   /> */}
      {/* ) : ( */}
      {/*   <Button */}
      {/*     theme={"green"} */}
      {/*     textContent="Create new" */}
      {/*     onClick={toggleViewCreateExperiment} */}
      {/*   /> */}
      {/* )} */}
    </>
  );
}
