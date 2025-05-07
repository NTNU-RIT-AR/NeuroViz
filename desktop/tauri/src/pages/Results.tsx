import { useEffect, useRef, useState } from "react";
import { Layout } from "../components/Layout";

import {
  DocumentMagnifyingGlassIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import { useMemo } from "react";
import {
  commands,
  ChoiceExperimentResult,
  RatingExperimentResult,
  ResultWithExperiment,
  WithKey,
  ExperimentResult,
} from "../bindings.gen";
import Button from "../components/Button";
import { Input, Label } from "../components/Input";
import Popup from "../components/Popup";
import { useCommand, useFuse } from "../hooks";
import styles from "./Results.module.css";
import { match } from "ts-pattern";

interface ResultCardProps {
  result: WithKey<ResultWithExperiment>;
  onDelete: () => void;
  onViewDetails: () => void;
}

function ResultCard(props: ResultCardProps) {
  const { onDelete, onViewDetails } = props;
  const { experiment_key, result } = props.result.value;

  // Format date string - assuming time is in ISO format
  const date = new Date(result.time).toLocaleDateString();

  // Determine result type
  const resultType = match(result)
    .with({ experiment_type: "rating" }, () => "Rating")
    .with({ experiment_type: "choice" }, () => "Choice")
    .exhaustive();

  // Get count of items
  const itemCount = match(result)
    .with({ experiment_type: "rating" }, (r) => r.ratings.length)
    .with({ experiment_type: "choice" }, (r) => r.choices.length)
    .exhaustive();

  return (
    <div className={styles.resultCard}>
      <h3 className={styles.resultCardTitle}>{result.name}</h3>
      <div className={styles.resultCardContent}>
        <p>Type: {resultType}</p>
        <p>Date: {date}</p>
        <p>Observer ID: {result.observer_id}</p>
        <p>Items: {itemCount}</p>
      </div>
      <div className={styles.resultCardBottom}>
        {/* Delete button */}
        <Button
          variant="danger"
          square={true}
          onClick={() => {
            if (confirm("Are you sure you want to delete this result?")) {
              onDelete();
            }
          }}
        >
          <TrashIcon className="icon" />
        </Button>

        <Button variant="primary" square={true} onClick={onViewDetails}>
          <DocumentMagnifyingGlassIcon className="icon" />
        </Button>
      </div>
    </div>
  );
}

interface ResultDetailsProps {
  result: WithKey<ResultWithExperiment>;
  onClose: () => void;
}

function ResultDetailsPopup(props: ResultDetailsProps) {
  const { result, onClose } = props;

  // Format date string - assuming time is in ISO format
  const date = new Date(result.value.result.time).toLocaleDateString();
  const time = new Date(result.value.result.time).toLocaleTimeString();

  const table = match(result.value.result)
    .with({ experiment_type: "rating" }, (result) => (
      <>
        <h3>Rating Results</h3>
        <table className={styles.resultDetailTable}>
          <thead>
            <tr>
              <th>Preset</th>
              <th>Rating</th>
              <th>Time</th>
              <th>Duration (ms)</th>
            </tr>
          </thead>
          <tbody>
            {result.ratings.map((rating, idx) => (
              <tr key={idx}>
                <td>{rating.preset}</td>
                <td>{rating.rank}</td>
                <td>{new Date(rating.time).toLocaleTimeString()}</td>
                <td>{rating.duration}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </>
    ))
    .with({ experiment_type: "choice" }, (result) => (
      <>
        <h3>Choice Results</h3>
        <table className={styles.resultDetailTable}>
          <thead>
            <tr>
              <th>Option A</th>
              <th>Option B</th>
              <th>Selected</th>
              <th>Time</th>
              <th>Duration (ms)</th>
            </tr>
          </thead>
          <tbody>
            {result.choices.map((choice, idx) => (
              <tr key={idx}>
                <td>{choice.a}</td>
                <td>{choice.b}</td>
                <td>{choice.selected}</td>
                <td>{new Date(choice.time).toLocaleTimeString()}</td>
                <td>{choice.duration}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </>
    ))
    .exhaustive();

  return (
    <Popup title="Result Details" onClose={onClose}>
      <div className={styles.resultDetails}>
        <div className={styles.resultDetailsHeader}>
          <h2>{result.value.result.name}</h2>
          <p>Observer ID: {result.value.result.observer_id}</p>
        </div>

        <div>
          <p>
            Date: {date} {time}
          </p>
          {result.value.result.note && <p>Note: {result.value.result.note}</p>}
        </div>

        <div className={styles.resultDetailContent}>{table}</div>
      </div>
    </Popup>
  );
}

export default function ResultsPage() {
  // Assuming a getResults command exists or will be implemented
  const results = useCommand(commands.getResults);
  const [selectedResult, setSelectedResult] =
    useState<WithKey<ResultWithExperiment> | null>(null);

  useEffect(() => {
    commands.setIdleMode();
  }, []);

  const [search, setSearch] = useState("");
  const filteredResults = useFuse(search, results.data, ["value.result.name"]);

  return (
    <>
      <Layout
        title="Results"
        folder="Results"
        toolbar={
          <div className={styles.headerRow}>
            <div className={styles.searchBoxWrapper}>
              <Input
                placeholder="Search results"
                value={search}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  setSearch(e.target.value)
                }
              />
            </div>
          </div>
        }
      >
        <div className={styles.resultsContainer}>
          {filteredResults && filteredResults.length > 0 ? (
            filteredResults.map((result) => (
              <ResultCard
                key={result.key}
                result={result}
                onViewDetails={() => setSelectedResult(result)}
                onDelete={async () => {
                  // Assuming a deleteResult command exists or will be implemented
                  await commands.deleteResult(
                    result.key,
                    result.value.experiment_key,
                  );
                  results.refetch();
                }}
              />
            ))
          ) : (
            <div>No results found. Run an experiment to generate results.</div>
          )}
        </div>
      </Layout>

      {selectedResult && (
        <ResultDetailsPopup
          result={selectedResult}
          onClose={() => setSelectedResult(null)}
        />
      )}
    </>
  );
}
