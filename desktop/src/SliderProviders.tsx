import { createContext, useContext, useState, ReactNode } from "react";

// Define context type
interface SliderContextType {
  hue: number;
  smoothness: number;
  metallic: number;
  emission: number;
  setHue: (value: number) => void;
  setSmoothness: (value: number) => void;
  setMetallic: (value: number) => void;
  setEmission: (value: number) => void;
}

// Create Context with default undefined value to enforce provider usage
const SliderContext = createContext<SliderContextType | undefined>(undefined);

// Provider Component Props
interface SliderProviderProps {
  children: ReactNode;
}

// Provider Component
export const SliderProvider: React.FC<SliderProviderProps> = ({ children }) => {
  const [hue, setHue] = useState<number>(1.0);
  const [smoothness, setSmoothness] = useState<number>(1.0);
  const [metallic, setMetallic] = useState<number>(1.0);
  const [emission, setEmission] = useState<number>(1.0);

  return (
    <SliderContext.Provider
      value={{
        hue,
        setHue,
        smoothness,
        setSmoothness,
        metallic,
        setMetallic,
        emission,
        setEmission,
      }}
    >
      {children}
    </SliderContext.Provider>
  );
};

// Custom hook to use the context
export const useSliders = (): SliderContextType => {
  const context = useContext(SliderContext);
  if (!context) {
    throw new Error("useSliders must be used within a SliderProvider");
  }
  return context;
};
