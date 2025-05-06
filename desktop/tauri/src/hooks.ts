import { useSuspenseQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { commands, events } from "./bindings.gen";

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
