import { useState } from "react";
import Backdrop from "./Backdrop";
import styles from "./styles/Popup.module.css";
import { XMarkIcon } from "@heroicons/react/24/outline";

interface PopupProps {
  title: string;
  children?: React.ReactNode;
  onClose: () => void;
}

export default function Popup({ children, title, onClose }: PopupProps) {
  return (
    <Backdrop>
      <div className={styles.popup}>
        <div className={styles.top}>
          <h1>{title}</h1>
          <XMarkIcon className={styles.close} onClick={onClose} />
        </div>
        {children}
      </div>
    </Backdrop>
  );
}
