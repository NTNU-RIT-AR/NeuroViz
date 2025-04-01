import { ParameterKey, Preset } from "./bindings.gen";


export interface Parameter {
  key: ParameterKey,
  name: string,
}

export interface Parameters {
  hue: number;
  smoothness: number;
  metallic: number;
  emission: number;
}

export interface Choice {
  a: string;
  b: string;
}

export type Experiment =
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

export interface QrPayload {
  ip: string;
  port: number;
  secret: string;
}

