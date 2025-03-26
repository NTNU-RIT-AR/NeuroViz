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

interface ExperimentCardProps {
  data: Experiment,
}

function viewExperiment() {
  return (
    <Backdrop><></></Backdrop>
  )
}

function ExperimentCard({ data }: ExperimentCardProps) {

  return (
    <button className={styles.experimentCard} onClick={viewExperiment}>
      <h3>{data.name}</h3>
      <p>Type: {data.experiment_type}</p>
    </button >
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
