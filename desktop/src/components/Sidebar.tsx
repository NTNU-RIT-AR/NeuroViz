import { NavLink } from "react-router-dom";
import { listen } from "@tauri-apps/api/event";
import { useEffect, useState } from "react";

import { DEEPLINK } from "../const.ts";
import ConnectionBox from "./ConnectionBox.tsx";
import "../App.css"
import styles from "./styles/Sidebar.module.css";
import { getIpAddress } from "../commands.ts";

export default function Sidebar() {
  const [deviceConnected, setDeviceConnected] = useState<boolean>(false);
  const [ipAddress, setIpAddress] = useState<string>("");

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

    async function getIp() {
      const ipAddress = await getIpAddress();
      console.log("ip address is ", ipAddress);

      if (ipAddress !== "") {
        setIpAddress(ipAddress);
      }
    }
    getIp();

    return () => {
      //Remove event listeners
      connectionEventListener.then((unlisten) => unlisten());
      disconnectionEventListener.then((unlisten) => unlisten());
    };
  }, []);

  return (
    <div className={styles.Sidebar}>
      {ipAddress !== "" && (
        <ConnectionBox
          // url={"https://www.youtube.com/watch?v=dQw4w9WgXcQ"}
          url={DEEPLINK + ipAddress}
          isConnected={deviceConnected}
        />
      )}
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
