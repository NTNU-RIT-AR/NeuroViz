import { useEffect, useState } from "react";
import { fetchExperiments, retrieveExperiment } from "../commands";
import { Layout } from "../components/Layout";
import { Experiment } from "../interfaces";
import { invoke } from "@tauri-apps/api/core";

import ExperimentCard from "../components/ExperimentCard";


export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<Experiment[]>([]);

  useEffect(() => {
    invoke<Experiment[]>("get_experiments").then(setExperiments);
  }, []);

  return (
    <>
      <h1>Experiments</h1>
      <Layout>
        {
          experiments.map((experiment) => (
            <ExperimentCard data={experiment} />

            // retrieveExperiment(experiment_name).then((result) =>
            //   setSelectedExperiment(result)
          ))
        }

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
