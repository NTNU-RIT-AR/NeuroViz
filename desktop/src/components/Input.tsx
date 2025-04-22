import classNames from "classnames";
import { ComponentProps } from "react";
import type StateManagedSelect from "react-select";
import ReactSelect from "react-select";
import styles from "./Input.module.css";

interface LabelProps {
  children: React.ReactNode;
}

export function Label(props: LabelProps) {
  const { children } = props;

  return <label className={styles.label}>{children}</label>;
}

export function Input(props: ComponentProps<"input">) {
  return (
    <input {...props} className={classNames(styles.input, props.className)} />
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
          color: "white",
        }),
        option: (base) => ({
          ...base,
          color: "rgba(0, 0, 0, 0.7)",
        }),
        singleValue: (base) => ({
          ...base,
          color: "rgba(255, 255, 255, 0.9)",
        }),
      }}
    />
  );
};
