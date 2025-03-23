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
  parameters: Parameters;
}

interface Experiment {
  experiment_type: string;
  name: string;
  presets: Map<string, Preset>;
  choices: Choice[];
}

// enum Parameter =
export type { Parameters, Preset, Experiment };
