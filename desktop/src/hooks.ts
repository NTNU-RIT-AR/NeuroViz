import { useSuspenseQuery } from "@tanstack/react-query";

export function useCommand<T>(command: () => Promise<T>) {
  return useSuspenseQuery({
    queryKey: [command.name],
    queryFn: command,
  });
}
