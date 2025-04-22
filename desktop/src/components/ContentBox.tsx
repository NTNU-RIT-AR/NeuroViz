import classNames from "classnames";
import styles from "./ContentBox.module.css";

interface ContentBoxProps {
  children?: React.ReactNode;
  className?: string;
}

export function ContentBox(props: ContentBoxProps) {
  return (
    <div className={classNames(styles.container, props.className)}>
      {props.children}
    </div>
  );
}
