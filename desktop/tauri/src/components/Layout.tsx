import { FolderIcon } from "@heroicons/react/24/outline";
import classNames from "classnames";
import { commands, TopLevelFolder } from "../bindings.gen";
import Button from "./Button";
import styles from "./Layout.module.css";

interface LayoutProps {
  title: string;
  className?: string;
  children?: React.ReactNode;
  toolbar?: React.ReactNode;
  folder?: TopLevelFolder;
}

export function Layout({
  title,
  className,
  children,
  toolbar,
  folder,
}: LayoutProps) {
  return (
    <>
      <main className={styles.layout}>
        <div className={styles.headerContainer}>
          <header className={styles.header}>
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
          </header>
        </div>
        <div className={styles.contentContainer}>
          <main className={classNames(styles.content, className)}>
            {children}
          </main>
        </div>
      </main>
    </>
  );
}
