import { QRCodeCanvas } from "qrcode.react";
import styles from "./ConnectionBox.module.css";
import xrealGlassesPng from "../assets/xreal-glasses.png";

interface ConnectionBoxProps {
  qrText: string;
  isConnected: boolean;
}

export default function ConnectionBox({
  qrText,
  isConnected,
}: ConnectionBoxProps) {
  let qrCode = (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        height: "100%",
        width: "100%",
      }}
    >
      <QRCodeCanvas
        className="qrcode"
        value={qrText}
        fgColor="#E43333"
        bgColor="#00000000"
        size={1000}
        style={{
          height: "87%",
          width: "87%",
          borderRadius: "6px",
          margin: "auto",
        }}
      />
    </div>
  );

  return (
    <div
      className={`${styles.box} ${
        isConnected ? styles.connected : styles.disconnected
      }`}
    >
      {isConnected ? (
        <>
          <img src={xrealGlassesPng} />
          <p>Connected</p>
        </>
      ) : (
        <>
          {qrCode}
          <p>Disconnected</p>
        </>
      )}
    </div>
  );
}
