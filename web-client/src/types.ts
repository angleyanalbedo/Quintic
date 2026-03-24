// Frontend Types

// Use 'as const' object instead of Enum for better type erasure compatibility
export const MotionLawType = {
  POLY5: "Polynomial5",
  MODIFIED_SINE: "ModifiedSine",
  CYCLOIDAL: "Cycloidal",
  CONSTANT_VELOCITY: "ConstantVelocity"
} as const;

export type MotionLawType = typeof MotionLawType[keyof typeof MotionLawType];

export interface Segment {
  id: string;
  motionLaw: MotionLawType;
  masterStart: number;
  masterEnd: number;
  slaveStart: number;
  slaveEnd: number;
}

export interface ProjectConfig {
  masterVelocity: number;
  resolution: number;
}

export interface Project {
  config: ProjectConfig;
  segments: Segment[];
}

export interface CamPoint {
  theta: number;
  s: number;
  v: number;
  a: number;
  j: number;
}
