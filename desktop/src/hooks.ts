import { useSuspenseQuery } from "@tanstack/react-query";

export function useCommand<T>(command: () => Promise<T>) {
  return useSuspenseQuery({
    queryKey: [command.name],
    queryFn: command,
    staleTime: 5 * (60 * 1000), // 5 minutes
  });
}
