import classNames from "classnames";
import styles from "./styles/Layout.module.css";

interface LayoutProps {
  title: string;
  scrollable?: boolean;
  className?: string;
  children: React.ReactNode;
  toolbar?: React.ReactNode;
}

export function Layout({
  title,
  scrollable = false,
  className,
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
          className={classNames(
            scrollable
              ? styles.contentContainerScrollable
              : styles.contentContainer,
            className
          )}
        >
          {children}
        </div>
      </main>
    </>
  );
}
