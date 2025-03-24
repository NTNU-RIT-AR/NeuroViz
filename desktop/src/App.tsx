import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import "./components/styles/Button.css";
import styles from "./App.module.css";
import { NavigationProvider } from "./NavigationProvider";
import { SliderProvider } from "./SliderProviders";
import Sidebar from "./components/Sidebar";
import {
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
} from "./const";
import ExperimentsPage from "./pages/Experiments";
import LiveViewPage from "./pages/LiveView";
import PresetsPage from "./pages/Presets";
import ResultsPage from "./pages/Results";

export default function App() {
  return (
    <>
      <BrowserRouter>
        <SliderProvider>
          <NavigationProvider>
            <div className={styles.mainLayout}>
              <Sidebar />
              <main className={styles.pageContainer}>
                <Routes>
                  <Route path="/" element={<></>} />
                  <Route path={ROUTE_LIVE_VIEW} element={<LiveViewPage />} />
                  <Route path={ROUTE_PRESETS} element={<PresetsPage />} />
                  <Route
                    path={ROUTE_EXPERIMENTS}
                    element={<ExperimentsPage />}
                  />
                  <Route path={ROUTE_RESULTS} element={<ResultsPage />} />
                </Routes>
              </main>
            </div>
          </NavigationProvider>
        </SliderProvider>
      </BrowserRouter>
    </>
  );
}
