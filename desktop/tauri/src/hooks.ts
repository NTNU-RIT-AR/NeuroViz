import { useSuspenseQuery } from "@tanstack/react-query";
import { use, useEffect, useState } from "react";
import { commands, events } from "./bindings.gen";
import { QrPayload } from "./components/Sidebar";
import { UNITY_API_PORT } from "./const";

export function useCommand<T>(command: () => Promise<T>) {
  return useSuspenseQuery({
    queryKey: [command.name],
    queryFn: command,
    staleTime: 5 * (60 * 1000), // 5 minutes
  });
}

export function useIsConnected() {
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    commands.isConnected().then(setIsConnected);

    const connectionEventListener = events.connectionEvent.listen((event) => {
      setIsConnected(event.payload.is_connected);
    });

    return () => {
      // Remove event listeners
      connectionEventListener.then((unlisten) => unlisten());
    };
  }, []);

  return isConnected;
}

const secretPromise = commands.getSecret();
const ipAddressPromise = commands.getIpAddress();

export function useConnectionQrCode() {
  const secret = use(secretPromise);
  const ipAddress = use(ipAddressPromise);

  const qrPayload: QrPayload = {
    ip: ipAddress,
    port: UNITY_API_PORT,
    secret,
  };

  const qrText = JSON.stringify(qrPayload);

  return qrText;
}
