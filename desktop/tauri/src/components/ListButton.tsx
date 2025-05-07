import styles from "./ListButton.module.css";

interface ListButtonProps {
  textContent: string;
  onClick: () => void;
}

export default function ListButton({ textContent, onClick }: ListButtonProps) {
  return (
    <button className={styles.listButton} onClick={onClick}>
      {textContent}
    </button>
  );
}
