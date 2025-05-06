import { NavLink } from "react-router-dom";
import {
  ROUTE_EXPERIMENTS,
  ROUTE_LIVE_VIEW,
  ROUTE_PRESETS,
  ROUTE_RESULTS,
} from "../const.ts";
import { useConnectionQrCode } from "../hooks.ts";
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
  const connectionQrCode = useConnectionQrCode();

  return (
    <div className={styles.Sidebar}>
      <ConnectionBox qrText={connectionQrCode} isConnected={isConnected} />

      <nav>
        <SidebarLink to={ROUTE_LIVE_VIEW}>Live View</SidebarLink>
        <SidebarLink to={ROUTE_PRESETS}>Presets</SidebarLink>
        <SidebarLink to={ROUTE_EXPERIMENTS}>Experiments</SidebarLink>
        <SidebarLink to={ROUTE_RESULTS}>Results</SidebarLink>
      </nav>
    </div>
  );
}
