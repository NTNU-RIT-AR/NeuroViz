import { useEffect, useState } from "react";
import { fetchExperiments } from "../commands";

export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<string[]>([""]);

  useEffect(() => {
    fetchExperiments().then(setExperiments);
  }, []);

  return (
    <>
      {experiments.length > 0 &&
        experiments.map((experiment_name) => <p>{experiment_name}</p>)}
    </>
  );
}
