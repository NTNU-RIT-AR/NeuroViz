import classNames from "classnames";
import styles from "./styles/Layout.module.css";

interface LayoutProps {
  title: string;
  scrollable?: boolean;
  className?: string;
  children: React.ReactNode;
}

export function Layout({
  title,
  scrollable = false,
  className,
  children,
}: LayoutProps) {
  return (
    <main className={styles.pageContainer}>
      <h1 className={styles.title}>{title}</h1>
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
  );
}
