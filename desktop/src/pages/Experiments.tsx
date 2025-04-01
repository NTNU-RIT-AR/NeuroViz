import { invoke } from "@tauri-apps/api/core";
import { useEffect, useState } from "react";
import { Layout } from "../components/Layout";

import { EyeIcon, PlayIcon, TrashIcon } from "@heroicons/react/24/outline";
import Button from "../components/Button";
import Popup from "../components/Popup";
import styles from "./styles/Experiments.module.css";
import { commands, Experiment } from "../bindings.gen";

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
  data: Experiment;
}
function ExperimentCard({ data }: ExperimentCardProps) {
  const [show, setShow] = useState(false);
  return (
    <>
      <div className={styles.experimentCard}>
        <h3 className={styles.experimentCardTitle}>{data.name}</h3>
        <div className={styles.experimentCardContent}>
          <p>Type: {data.experiment_type}</p>
        </div>
        <div className={styles.experimentCardBottom}>
          <Button square={true} onClick={() => setShow(true)}>
            {" "}
            <EyeIcon className="icon" />
          </Button>
          <Button square={true}>
            <TrashIcon />
          </Button>
          <Button square={true}>
            <PlayIcon />
          </Button>{" "}
        </div>
      </div>

      {show && <ViewExperiment data={data} setShow={setShow} />}
    </>
  );
}

export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<Experiment[]>([]);
  const [filter, setFilter] = useState<FilterState>(FilterState.All);

  useEffect(() => {
    commands.getAllExperiments().then(setExperiments);
    invoke<Experiment[]>("get_all_experiments").then(setExperiments);
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
              <ExperimentCard data={experiment} />
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
