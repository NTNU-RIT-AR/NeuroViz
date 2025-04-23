import { XMarkIcon } from "@heroicons/react/24/outline";
import { createPortal } from "react-dom";
import styles from "./Popup.module.css";

interface PopupProps {
  title: string;
  children?: React.ReactNode;
  onClose: () => void;
}

export default function Popup({ children, title, onClose }: PopupProps) {
  return createPortal(
    <>
      <div className={styles.backdrop} onClick={onClose} />
      <div className={styles.popup}>
        <div className={styles.top}>
          <h1>{title}</h1>
          <XMarkIcon
            className={styles.close}
            onClick={onClose}
            style={{ width: 48, height: 48 }}
          />
        </div>
        {children}
      </div>
    </>,
    document.body
  );
}
