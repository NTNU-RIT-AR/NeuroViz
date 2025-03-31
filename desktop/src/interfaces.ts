interface Parameter {
  key: string,
  name: string,
  value: number,
}


interface Parameters {
  hue: number;
  smoothness: number;
  metallic: number;
  emission: number;
}

interface Choice {
  a: string;
  b: string;
}

interface Preset {
  name: string;
  parameters: Parameter[];
}

type Experiment =
  | {
    experiment_type: "choice";
    name: string;
    presets: Record<string, Preset>;
    choices: Choice[];
  }
  | {
    experiment_type: "rating";
    name: string;
    presets: Record<string, Preset>;
  };

// enum Parameter =
export type { Parameters, Preset, Experiment };
