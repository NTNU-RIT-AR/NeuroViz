import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useCallback, useEffect, useState } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import "./App.css";
import styles from "./App.module.css";
import { AppState, commands, events, ExperimentState } from "./bindings.gen";
import Sidebar from "./components/Sidebar";
import {
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
} from "./const";
import { ActiveExperiment } from "./pages/ActiveExperiment";
import ExperimentsPage from "./pages/Experiments";
import LiveViewPage from "./pages/LiveView";
import PresetsPage from "./pages/Presets";
import ResultsPage from "./pages/Results";

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

const queryClient = new QueryClient();

export default function App() {
  const experimentState = useExperimentState();

  if (experimentState) {
    return <ActiveExperiment experimentState={experimentState} />;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className={styles.mainLayout}>
          <Sidebar />
          <Routes>
            <Route index element={<Navigate to={ROUTE_LIVE_VIEW} />} />
            <Route path={ROUTE_LIVE_VIEW} element={<LiveViewPage />} />
            <Route path={ROUTE_PRESETS} element={<PresetsPage />} />
            <Route path={ROUTE_EXPERIMENTS} element={<ExperimentsPage />} />
            <Route path={ROUTE_RESULTS} element={<ResultsPage />} />
          </Routes>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
