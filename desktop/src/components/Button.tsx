interface ButtonProps {
  theme: "green" | "red" | "neutral";
  textContent: string;
  onClick: () => void;
}

export default function Button({ theme, textContent, onClick }: ButtonProps) {
  return (
    <button onClick={onClick} className={theme}>
      {textContent}
    </button>
  );
}
