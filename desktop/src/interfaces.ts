import { Preset } from "./bindings.gen";

interface Parameter {
  key: string;
  name: string;
  value: number;
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

interface QrPayload {
  ip: string;
  port: number;
  secret: string;
}

export type { Experiment, Parameters, Preset, QrPayload };
