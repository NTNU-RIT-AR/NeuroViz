import { NavLink } from "react-router-dom";
import { listen } from "@tauri-apps/api/event";
import { useEffect, useState } from "react";

import {
  DEEPLINK,
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
} from "../const.ts";
import ConnectionBox from "./ConnectionBox.tsx";
import styles from "./styles/Sidebar.module.css";
import { getIpAddress } from "../commands.ts";
import { useAppNavigate } from "./../NavigationProvider.tsx";

export default function Sidebar() {
  const [deviceConnected, setDeviceConnected] = useState<boolean>(false);
  const [ipAddress, setIpAddress] = useState<string>("");

  const navigate = useAppNavigate();

  useEffect(() => {
    const connectionEventListener = listen<string>("connection", (event) => {
      console.log("Device connected", event.payload);
      setDeviceConnected(true);
      navigate(ROUTE_LIVE_VIEW);
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
          to={ROUTE_LIVE_VIEW}
        >
          Live View
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to={ROUTE_PRESETS}
        >
          Presets
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to={ROUTE_EXPERIMENTS}
        >
          Questionaires
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            `${styles.Link} ${isActive ? styles.active : ""}`
          }
          to={ROUTE_RESULTS}
        >
          Results
        </NavLink>
      </nav>
    </div>
  );
}
