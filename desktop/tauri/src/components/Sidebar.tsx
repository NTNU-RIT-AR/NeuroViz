import { useState } from "react";
import { NavLink } from "react-router-dom";
import { commands } from "../bindings.gen.ts";
import {
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
  UNITY_API_PORT,
} from "../const.ts";
import { useCommand } from "../hooks.ts";
import ConnectionBox from "./ConnectionBox.tsx";
import styles from "./Sidebar.module.css";

export interface QrPayload {
  ip: string;
  port: number;
  secret: string;
}

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

interface SidebarProps {
  isConnected: boolean;
}

export default function Sidebar(props: SidebarProps) {
  const { isConnected } = props;

  const secret = useCommand(commands.getSecret).data;
  const [ipAddress, setIpAddress] = useState<string>("");

  const qrPayload: QrPayload = {
    ip: ipAddress,
    port: UNITY_API_PORT,
    secret,
  };

  return (
    <div className={styles.Sidebar}>
      {
        <ConnectionBox
          // url={"https://www.youtube.com/watch?v=dQw4w9WgXcQ"}
          qrText={JSON.stringify(qrPayload)}
          isConnected={isConnected}
        />
      }
      <nav>
        <SidebarLink to={ROUTE_LIVE_VIEW}>Live View</SidebarLink>
        <SidebarLink to={ROUTE_PRESETS}>Presets</SidebarLink>
        <SidebarLink to={ROUTE_EXPERIMENTS}>Experiments</SidebarLink>
        <SidebarLink to={ROUTE_RESULTS}>Results</SidebarLink>
      </nav>
    </div>
  );
}
