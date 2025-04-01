interface Parameter {
  key: string,
  name: string,
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
export type { Parameter, Preset, Experiment };
