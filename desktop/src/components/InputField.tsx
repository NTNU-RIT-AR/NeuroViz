import styles from "./InputField.module.css";

interface LabelProps {
  children: React.ReactNode;
}

export function Label(props: LabelProps) {
  const { children } = props;

  return <label className={styles.label}>{children}</label>;
}

interface InputProps<T> {
  ref: React.RefObject<T | null>;
}

export function TextInput(props: InputProps<HTMLInputElement>) {
  const { ref } = props;

  return <input ref={ref} className={styles.input} type="text" />;
}

export function NumberInput(props: InputProps<HTMLInputElement>) {
  const { ref } = props;

  return <input ref={ref} className={styles.input} type="number" />;
}

export function TextArea(props: InputProps<HTMLTextAreaElement>) {
  const { ref } = props;

  return <textarea ref={ref} className={styles.input} />;
}
