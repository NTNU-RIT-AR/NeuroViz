import classNames from "classnames";
import { ComponentProps } from "react";
import type StateManagedSelect from "react-select";
import ReactSelect from "react-select";
import styles from "./Input.module.css";

interface LabelProps {
  children: React.ReactNode;
  horizontal?: boolean;
}

export function Label(props: LabelProps) {
  const { children, horizontal } = props;

  return (
    <label
      className={classNames(styles.label, horizontal && styles.horizontal)}
    >
      {children}
    </label>
  );
}

export function Input(props: ComponentProps<"input">) {
  return (
    <input {...props} className={classNames(styles.input, props.className)} />
  );
}

export function Checkbox(props: ComponentProps<"input">) {
  return (
    <input
      {...props}
      className={classNames(styles.input, styles.checkbox, props.className)}
      type="checkbox"
    />
  );
}

export function TextArea(props: ComponentProps<"textarea">) {
  return (
    <textarea
      {...props}
      className={classNames(styles.input, props.className)}
    />
  );
}

export const Select: StateManagedSelect = (props) => {
  return (
    <ReactSelect
      {...props}
      styles={{
        control: (base) => ({
          ...base,
          backgroundColor: "var(--primary-bg)",
          border: "solid var(--line) 2px",
          cursor: "pointer",
        }),
        menu: (base) => ({
          ...base,
          background: "var(--bg)",
          color: "#ffffffaa",
          border: "2px solid var(--line)",
        }),
        option: (base) => ({
          ...base,
          background: "var(--primary-bg)",
          cursor: "pointer",
          borderBottom: "1px solid var(--line)",
        }),
        singleValue: (base) => ({
          ...base,
          color: "rgba(255, 255, 255, 0.9)",
        }),
      }}
    />
  );
};
