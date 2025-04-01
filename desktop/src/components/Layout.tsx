import styles from "./styles/Layout.module.css";

interface LayoutProps {
  title: string;
  scrollable?: boolean;
  children: React.ReactNode;
  toolbar?: React.ReactNode;
}

export function Layout({
  title,
  scrollable = false,
  children,
  toolbar,
}: LayoutProps) {
  return (
    <>
      <main className={styles.layout}>
        <div className={styles.header}>
          <h1 className={styles.title}>{title}</h1>
          <div className={styles.toolbar}>{toolbar && toolbar}</div>
        </div>
        <div
          className={
            scrollable
              ? styles.contentContainerScrollable
              : styles.contentContainer
          }
        >
          {children}
        </div>
      </main>
    </>
  );
}
