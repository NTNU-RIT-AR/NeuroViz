import classNames from "classnames";
import styles from "./Button.module.css";

type Variant = "primary" | "secondary" | "danger";

interface ButtonProps {
  className?: string;
  variant?: Variant;
  onClick?: () => void;
  children?: React.ReactNode;
  square?: boolean;
}

const styleVariant: Record<Variant, string> = {
  primary: styles.primary,
  secondary: styles.secondary,
  danger: styles.danger,
};

export default function Button({
  className,
  children,
  variant,
  onClick,
  square = false,
}: ButtonProps) {
  return (
    <button
      onClick={onClick}
      className={classNames(
        className,
        styles.button,
        square && styles.square,
        variant && styleVariant[variant]
      )}
    >
      {children}
    </button>
  );
}
