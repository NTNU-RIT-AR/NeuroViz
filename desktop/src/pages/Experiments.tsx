import { useEffect, useState } from "react";
import { fetchExperiments, retrieveExperiment } from "../commands";
import { Layout } from "../components/Layout";
import { Experiment } from "../interfaces";
import { invoke } from "@tauri-apps/api/core";

import styles from "./styles/Experiments.module.css"
import Backdrop from "../components/Backdrop";

enum FilterState {
  All = "All",
  Published = "Published",
  Draft = "Draft"
}

interface ViewExperimentProps {
  data: Experiment
  setShow: React.Dispatch<React.SetStateAction<boolean>>
}

function ViewExperiment({ data, setShow }: ViewExperimentProps) {
  return (
    <Backdrop>
      <h3>{data.name}</h3>
      <button onClick={() => setShow(false)}>Close</button>
    </Backdrop >
  )
}

interface ExperimentCardProps {
  data: Experiment,
}
function ExperimentCard({ data }: ExperimentCardProps) {
  const [show, setShow] = useState(false);
  return (
    <>
      <div className={styles.experimentCard} onClick={() => setShow(true)}>
        <h3 className={styles.experimentCardTitle}>{data.name}</h3>
        <div className={styles.experimentCardContent}>
          <p>Type: {data.experiment_type}</p>
        </div>
        <div className={styles.experimentCardBottom}></div>
      </div>

      {
        show &&
        <ViewExperiment data={data} setShow={setShow} />
      }
    </>
  )
}

export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<Experiment[]>([]);
  const [filter, setFilter] = useState<FilterState>(FilterState.All);

  useEffect(() => {
    invoke<Experiment[]>("get_all_experiments").then(setExperiments);
  }, []);

  return (
    <>
      <Layout title="Experiments" scrollable={true}>
        <div className={styles.experimentsContainer}>
          {
            experiments
              // .filter(
              //   (experiment) => (
              //     experiment
              //   )
              // )
              .map(
                (experiment) => (
                  <ExperimentCard data={experiment} />
                )
              )
          }
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
