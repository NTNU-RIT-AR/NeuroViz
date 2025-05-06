import "./App.css";
import styles from "./App.module.css";

import { lazy, Suspense, useCallback, useEffect, useState } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AppState, commands, events, ExperimentState } from "./bindings.gen";
import {
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
} from "./const";
import { useLiveParameters } from "./pages/LiveView";

const Sidebar = lazy(() => import("./components/Sidebar"));
const ExperimentsPage = lazy(() => import("./pages/Experiments"));
const ActiveExperiment = lazy(
  () => import("./pages/ActiveExperiment/ActiveExperiment")
);
const LiveViewPage = lazy(() => import("./pages/LiveView"));
const PresetsPage = lazy(() => import("./pages/Presets"));
const ResultsPage = lazy(() => import("./pages/Results"));

function useExperimentState(): ExperimentState | undefined {
  const [experimentState, setExperimentState] = useState<
    ExperimentState | undefined
  >(undefined);

  const setState = useCallback(
    (state: AppState) => {
      if (state.kind === "experiment") {
        setExperimentState(state);
      } else {
        setExperimentState(undefined);
      }
    },
    [setExperimentState]
  );

  useEffect(() => {
    commands.currentState().then(setState);

    const stateEventListener = events.stateEvent.listen((event) =>
      setState(event.payload.state)
    );

    // Cleanup the event listener when the component unmounts
    return () => {
      stateEventListener.then((unlisten) => unlisten());
    };
  }, []);

  return experimentState;
}

export default function App() {
  const experimentState = useExperimentState();
  const liveParameters = useLiveParameters();

  useEffect(() => {
    const unlisten = events.resultSavedEvent.listen((event) => {
      const { result_file_path } = event.payload;
      alert(
        `Result saved to ${result_file_path}. You can view it in the Results tab.`
      );
    });

    return () => {
      unlisten.then((unlisten) => unlisten());
    };
  }, []);

  if (experimentState) {
    return <ActiveExperiment experimentState={experimentState} />;
  }

  return (
    <BrowserRouter>
      <div className={styles.mainLayout}>
        <Suspense>
          <Sidebar />
        </Suspense>
        <Routes>
          <Route index element={<Navigate to={ROUTE_LIVE_VIEW} />} />
          <Route
            path={ROUTE_LIVE_VIEW}
            element={
              <Suspense>
                <LiveViewPage liveParameters={liveParameters} />
              </Suspense>
            }
          />
          <Route
            path={ROUTE_PRESETS}
            element={
              <Suspense>
                <PresetsPage />
              </Suspense>
            }
          />
          <Route
            path={ROUTE_EXPERIMENTS}
            element={
              <Suspense>
                <ExperimentsPage />
              </Suspense>
            }
          />
          <Route
            path={ROUTE_RESULTS}
            element={
              <Suspense>
                <ResultsPage />
              </Suspense>
            }
          />
        </Routes>
      </div>
    </BrowserRouter>
  );
}
