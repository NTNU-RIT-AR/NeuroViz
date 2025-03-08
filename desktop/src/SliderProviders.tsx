import { createContext, useContext, useState, ReactNode } from "react";

// Define context type
interface SliderContextType {
  slider1: number;
  slider2: number;
  slider3: number;
  slider4: number;
  setSlider1: (value: number) => void;
  setSlider2: (value: number) => void;
  setSlider3: (value: number) => void;
  setSlider4: (value: number) => void;
}

// Create Context with default undefined value to enforce provider usage
const SliderContext = createContext<SliderContextType | undefined>(undefined);

// Provider Component Props
interface SliderProviderProps {
  children: ReactNode;
}

// Provider Component
export const SliderProvider: React.FC<SliderProviderProps> = ({ children }) => {
  const [slider1, setSlider1] = useState<number>(1.0);
  const [slider2, setSlider2] = useState<number>(1.0);
  const [slider3, setSlider3] = useState<number>(1.0);
  const [slider4, setSlider4] = useState<number>(1.0);

  return (
    <SliderContext.Provider
      value={{
        slider1,
        setSlider1,
        slider2,
        setSlider2,
        slider3,
        setSlider3,
        slider4,
        setSlider4,
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
