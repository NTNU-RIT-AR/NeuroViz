import { NavLink } from "react-router-dom";
import ConnectionBox from "./ConnectionBox.tsx";

import styles from "./styles/Sidebar.module.css";

export default function Sidebar() {
  return (
    <div className={styles.Sidebar}>
      <ConnectionBox url={"https://www.youtube.com/watch?v=dQw4w9WgXcQ"} isConnected={true} />
      <nav>
        <NavLink className={({ isActive }) => `${styles.Link} ${isActive ? styles.active : ""}`} to="/live-view">
          Live View
        </NavLink>
        <NavLink className={({ isActive }) => `${styles.Link} ${isActive ? styles.active : ""}`} to="/presets">
          Presets
        </NavLink>
        <NavLink className={({ isActive }) => `${styles.Link} ${isActive ? styles.active : ""}`} to="/questionaires">
          Questionaires
        </NavLink>
        <NavLink className={({ isActive }) => `${styles.Link} ${isActive ? styles.active : ""}`} to="/results">
          Results
        </NavLink>
      </nav>
    </div>
  );
}
