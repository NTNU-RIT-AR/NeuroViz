import "./App.css";
import Sidebar from "./components/Sidebar";
import { BrowserRouter, Route, Routes, useNavigate } from "react-router-dom";
import LiveViewPage from "./pages/LiveView";
import PresetsPage from "./pages/Presets";
import QuestionairesPage from "./pages/Questionaires";
import ResultsPage from "./pages/Results";
import { useEffect } from "react";

function AutoNavigate() {
  const navigate = useNavigate();

  useEffect(() => {
    // Navigate to the desired page on component mount
    navigate("/live-view");
  }, [navigate]);

  return null; // No UI for the redirect component
}

export default function App() {
  return (
    <>
      <BrowserRouter>
        <div className="MainLayout">
          <Sidebar />
          <main className="container">
            <Routes>
              <Route path="/live-view" element={<LiveViewPage />} />
              <Route path="/presets" element={<PresetsPage />} />
              <Route path="/questionaires" element={<QuestionairesPage />} />
              <Route path="/results" element={<ResultsPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </>
  );
}
