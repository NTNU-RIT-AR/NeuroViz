import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { commands, events } from "../bindings.gen.ts";
import {
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
  UNITY_API_PORT,
} from "../const.ts";
import { QrPayload } from "../interfaces.ts";
import ConnectionBox from "./ConnectionBox.tsx";
import styles from "./styles/Sidebar.module.css";

interface SidebarLinkProps {
  to: string;
  children: React.ReactNode;
}

function SidebarLink(props: SidebarLinkProps) {
  return (
    <NavLink
      className={({ isActive }) =>
        `${styles.Link} ${isActive ? styles.active : ""}`
      }
      to={props.to}
    >
      {props.children}
    </NavLink>
  );
}

export default function Sidebar() {
  const [deviceConnected, setDeviceConnected] = useState<boolean>(false);
  const [ipAddress, setIpAddress] = useState<string>("");

  useEffect(() => {
    const connectionEventListener = events.connectionEvent.listen((event) => {
      setDeviceConnected(event.payload.is_connected);
    });

    async function getIp() {
      const ipAddress = await commands.getIpAddress();
      console.log("ip address is ", ipAddress);

      if (ipAddress !== "") {
        setIpAddress(ipAddress);
      }
    }
    getIp();

    return () => {
      //Remove event listeners
      connectionEventListener.then((unlisten) => unlisten());
    };
  }, []);

  const qrPayload: QrPayload = {
    ip: ipAddress,
    port: UNITY_API_PORT,
    // TODO: Use password protection
    secret: "",
  };

  return (
    <div className={styles.Sidebar}>
      {ipAddress !== "" && (
        <ConnectionBox
          // url={"https://www.youtube.com/watch?v=dQw4w9WgXcQ"}
          qrText={JSON.stringify(qrPayload)}
          isConnected={deviceConnected}
        />
      )}
      <nav>
        <SidebarLink to={ROUTE_LIVE_VIEW}>Live View</SidebarLink>
        <SidebarLink to={ROUTE_PRESETS}>Presets</SidebarLink>
        <SidebarLink to={ROUTE_EXPERIMENTS}>Experiments</SidebarLink>
        <SidebarLink to={ROUTE_RESULTS}>Results</SidebarLink>
      </nav>
    </div>
  );
}
