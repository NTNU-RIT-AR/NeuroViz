import { FolderIcon } from "@heroicons/react/24/outline";
import classNames from "classnames";
import { commands, TopLevelFolder } from "../bindings.gen";
import Button from "./Button";
import styles from "./styles/Layout.module.css";

interface LayoutProps {
  title: string;
  scrollable?: boolean;
  className?: string;
  children?: React.ReactNode;
  toolbar?: React.ReactNode;
  folder?: TopLevelFolder;
}

export function Layout({
  title,
  scrollable = false,
  className,
  children,
  toolbar,
  folder,
}: LayoutProps) {
  return (
    <>
      <main className={styles.layout}>
        <div className={styles.header}>
          {folder && (
            <Button
              square
              onClick={() => {
                commands.showFolder(folder);
              }}
            >
              <FolderIcon />
            </Button>
          )}
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
