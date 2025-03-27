import styles from "./styles/Button.module.css"


interface ButtonProps {
  variant?: "primary" | "secondary" | "danger",
  onClick?: () => void,
  children?: React.ReactNode,
  square?: boolean,
}



export default function Button({ variant, onClick }: ButtonProps) {
  let styleVariant = new Map<string, string>();
  styleVariant.set("primary", styles.primary)
  styleVariant.set("secondary", styles.secondary)
  styleVariant.set("danger", styles.danger)

  return (

    <button
      onClick={onClick}
      className={`${styles.button} ${styleVariant.get(variant || "primary")}`}
    >
    </button>
  );
} 
