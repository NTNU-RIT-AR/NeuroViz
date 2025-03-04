import { Link } from "react-router-dom";
import ConnectionBox from "./ConnectionBox.tsx";

import styles from "./Sidebar.module.css"

export default function Sidebar() {
  return (
    <div className={styles.Sidebar}>
      <ConnectionBox url={"https://www.youtube.com/watch?v=dQw4w9WgXcQ"} isConnected={true} />
      <nav>
        <Link className={styles.Link} to="live-view">Live View</Link>
        <Link className={styles.Link} to="presets">Presets</Link>
        <Link className={styles.Link} to="questionaires">Questionaires</Link>
        <Link className={styles.Link} to="results">Results</Link>
      </nav>
    </div>
  );
}
