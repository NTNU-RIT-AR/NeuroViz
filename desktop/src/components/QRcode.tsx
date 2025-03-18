import { QRCodeCanvas } from "qrcode.react";

export default function QRcode() {
  return (
    <QRCodeCanvas value={"https://www.vg.no/"} size={200} />
  );
}
