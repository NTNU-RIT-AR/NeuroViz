import styles from "./styles/Layout.module.css";

interface LayoutProps {
  title: string,
  scrollable?: boolean,
  children: React.ReactNode;
}

export function Layout({ title, scrollable = false, children }: LayoutProps) {
  return (
    <>
      <main className={styles.pageContainer}>
        <h1 className={styles.title}>{title}</h1 >
        <div className={scrollable ? styles.contentContainerScrollable : styles.contentContainer}>{children}</div>
      </main >
    </>
  );
}
