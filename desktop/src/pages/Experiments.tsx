import { useEffect, useState } from "react";
import { fetchExperiments, retrieveExperiment } from "../commands";

export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<string[]>([""]);
  const [selectedExperiment, setSelectedExperiment] = useState<
    string | undefined
  >("");

  useEffect(() => {
    fetchExperiments().then(setExperiments);
  }, []);

  return (
    <>
      {experiments.length > 0 &&
        experiments.map((experiment_name) => (
          <button
            onClick={() => {
              retrieveExperiment(experiment_name.split(".")[0]).then((result) =>
                setSelectedExperiment(result)
              );
            }}
          >
            {experiment_name}
          </button>
        ))}
      <p>{selectedExperiment && selectedExperiment}</p>
    </>
  );
}
