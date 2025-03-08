import "./App.css";
import Sidebar from "./components/Sidebar";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import LiveViewPage from "./pages/LiveView";
import PresetsPage from "./pages/Presets";
import ExperimentsPage from "./pages/Questionaires";
import ResultsPage from "./pages/Results";
import { NavigationProvider } from "./NavigationProvider";
import {
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_EXPERIMENTS,
  ROUTE_RESULTS,
} from "./const";

export default function App() {
  return (
    <>
      <BrowserRouter>
        <NavigationProvider>
          <div className="MainLayout">
            <Sidebar />
            <main className="container">
              <Routes>
                <Route path="/" element={<></>} />
                <Route path={ROUTE_LIVE_VIEW} element={<LiveViewPage />} />
                <Route path={ROUTE_PRESETS} element={<PresetsPage />} />
                <Route path={ROUTE_EXPERIMENTS} element={<ExperimentsPage />} />
                <Route path={ROUTE_RESULTS} element={<ResultsPage />} />
              </Routes>
            </main>
          </div>
        </NavigationProvider>
      </BrowserRouter>
    </>
  );
}
