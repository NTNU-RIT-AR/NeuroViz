import { Experiment } from "../interfaces";
import styles from "./styles/ExperimentCard.module.css";

interface ExperimentCardProps {
  data: Experiment,
}


export default function ExperimentCard({ data }: ExperimentCardProps) {
  if (true) {
    return (<></>)
  }
  return (
    <button className={styles.Card}>
      <h3>{data.name}</h3>
      {data.experiment_type}
    </button>
  )
}
