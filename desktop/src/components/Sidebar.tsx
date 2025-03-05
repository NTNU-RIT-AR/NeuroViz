import { NavLink } from "react-router-dom";
import ConnectionBox from "./ConnectionBox.tsx";
import { listen } from "@tauri-apps/api/event";

import styles from "./styles/Sidebar.module.css";
import { useEffect, useState } from "react";

export default function Sidebar() {
  const [deviceConnected, setDeviceConnected] = useState<boolean>(false);

  useEffect(() => {
    const connectionEventListener = listen<string>("connection", (event) => {
      console.log("Device connected", event.payload);
      setDeviceConnected(true);
    });

    const disconnectionEventListener = listen<string>(
      "disconnection",
      (event) => {
        console.log("Device disconnected", event.payload);
        setDeviceConnected(false);
      }
    );

    return () => {
      //Remove event listeners
      connectionEventListener.then((unlisten) => unlisten());
      disconnectionEventListener.then((unlisten) => unlisten());
    };
  }, []);

  return (
    <div className={styles.Sidebar}>
      <ConnectionBox
        url={"https://www.youtube.com/watch?v=dQw4w9WgXcQ"}
        isConnected={deviceConnected}
      />
      <nav>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to="/live-view"
        >
          Live View
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to="/presets"
        >
          Presets
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to="/questionaires"
        >
          Questionaires
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to="/results"
        >
          Results
        </NavLink>
      </nav>
    </div>
  );
}
