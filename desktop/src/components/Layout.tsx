import styles from "./styles/Layout.module.css";

interface LayoutProps {
  children: React.ReactNode;
}

export function Layout(props: LayoutProps) {
  return <div className={styles.container}>{props.children}</div>;
}
