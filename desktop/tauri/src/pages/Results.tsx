import { useEffect } from "react";
import { commands } from "../bindings.gen";
import { Layout } from "../components/Layout";

export default function ResultsPage() {
  useEffect(() => {
    commands.setIdleMode();
  }, []);

  return <Layout title="Results" folder="Results"></Layout>;
}
