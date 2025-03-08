import { createContext, useContext, ReactNode } from "react";
import { useNavigate, NavigateFunction } from "react-router-dom";

// Define the context with a proper type
const NavigationContext = createContext<NavigateFunction | undefined>(
  undefined
);

interface NavigationProviderProps {
  children: ReactNode;
}

export function NavigationProvider({ children }: NavigationProviderProps) {
  const navigate = useNavigate();
  return (
    <NavigationContext.Provider value={navigate}>
      {children}
    </NavigationContext.Provider>
  );
}

export function useAppNavigate(): NavigateFunction {
  const context = useContext(NavigationContext);
  if (!context) {
    throw new Error("useAppNavigate must be used within a NavigationProvider");
  }
  return context;
}
