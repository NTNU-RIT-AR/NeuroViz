import { useSuspenseQuery } from "@tanstack/react-query";
import { use, useEffect, useMemo, useState } from "react";
import { commands, events } from "./bindings.gen";
import { QrPayload } from "./components/Sidebar";
import { UNITY_API_PORT } from "./const";
import Fuse, { IFuseOptions } from "fuse.js";

/**
 * A hook that wraps a command function with React Query's useSuspenseQuery.
 * This provides caching, automatic refetching, and suspense support.
 */
export function useCommand<T>(command: () => Promise<T>) {
  return useSuspenseQuery({
    queryKey: [command.name],
    queryFn: command,
  });
}

/**
 * A hook that tracks the connection status.
 * It listens to connection events and updates state accordingly.
 *
 * @returns A boolean indicating whether the application is currently connected
 */
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

/**
 * A hook that generates a QR code payload for connection.
 * It combines IP address, port, and secret into a JSON string.
 *
 * @returns A JSON string containing connection information for QR code generation
 */
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

/**
 * A hook that implements fuzzy search functionality using Fuse.js.
 * If the search term is empty, returns the original data.
 *
 * @param searchTerm - The search query string
 * @param data - The array of items to search through
 * @param keys - The properties of the items to search on
 * @returns Filtered items that match the search term
 */
export function useFuse<T>(searchTerm: string, data: T[], keys: string[]) {
  const fuse = useMemo(
    () =>
      new Fuse(data, {
        keys,
        threshold: 0.3,
      }),
    [data, keys],
  );

  const results = useMemo(() => {
    if (searchTerm.trim().length === 0) return data;
    return fuse.search(searchTerm).map((res) => res.item);
  }, [searchTerm, fuse, data]);

  return results;
}
