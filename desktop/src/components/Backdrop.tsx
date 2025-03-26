import { createPortal } from "react-dom"
import styles from "./styles/Backdrop.module.css"

interface BackdropProps {
  children: React.ReactNode,
}

export default function Backdrop({ children }: BackdropProps) {
  return (
    createPortal(
      <div className={styles.Backdrop}>
        {children}
      </div>,
      document.body
    )
  )
}
