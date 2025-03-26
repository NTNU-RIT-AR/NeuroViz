import { useEffect, useState } from "react";
import { fetchExperiments, retrieveExperiment } from "../commands";
import ListButton from "../components/ListButton";
import { Layout } from "../components/Layout";
import { ContentBox } from "../components/ContentBox";
import CreateExperimentForm from "../components/CreateExperimentForm";
import Button from "../components/Button";
import { Experiment } from "../interfaces";
import { invoke } from "@tauri-apps/api/core";

export default function ExperimentsPage() {
  const [experiments, setExperiments] = useState<Experiment[]>([]);
  const [selectedExperiment, setSelectedExperiment] = useState<
    string | undefined
  >("");
  const [viewCreateExperiment, setViewCreateExperiment] =
    useState<boolean>(false);

  function toggleViewCreateExperiment() {
    setViewCreateExperiment(!viewCreateExperiment);
  }

  useEffect(() => {
    invoke<Experiment[]>("get_experiments").then(setExperiments);
  }, []);

  return (
    <>
      <h1>Experiments</h1>
      <Layout>
        <ContentBox>
          {experiments.length > 0 &&
            experiments.map((experiment_name) => (
              <ListButton
                textContent={experiment_name}
                onClick={() => {
                  retrieveExperiment(experiment_name).then((result) =>
                    setSelectedExperiment(result)
                  );
                }}
              />
            ))}
        </ContentBox>

        <ContentBox>{selectedExperiment && selectedExperiment}</ContentBox>
      </Layout>
      {viewCreateExperiment && <CreateExperimentForm />}
      {viewCreateExperiment ? (
        <Button
          theme={"neutral"}
          textContent="Cancel"
          onClick={toggleViewCreateExperiment}
        />
      ) : (
        <Button
          theme={"green"}
          textContent="Create new"
          onClick={toggleViewCreateExperiment}
        />
      )}
    </>
  );
}
