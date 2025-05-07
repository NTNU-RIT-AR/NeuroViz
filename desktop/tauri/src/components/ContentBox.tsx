import classNames from "classnames";
import styles from "./ContentBox.module.css";

interface ContentBoxProps extends React.HTMLAttributes<HTMLDivElement> {
  children?: React.ReactNode;
  className?: string;
}

export function ContentBox({ children, className, ...rest }: ContentBoxProps) {
  return (
    <div className={classNames(styles.container, className)} {...rest}>
      {children}
    </div>
  );
}
