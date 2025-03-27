import styles from "./styles/Buttom.module.css"


interface ButtonProps {
  variant?: "green" | "red" | "neutral",
  textContent: string,
  onClick: () => void,
  children: React.ReactNode,
}


export function IconButton() {
}

export function TextButton({ variant, textContent, onClick }: ButtonProps) {
  return (

    <button
      onClick={onClick}
      className={styles.button}
    >
      {textContent}
    </button>
  );
} Experiments
