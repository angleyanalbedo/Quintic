// Frontend Types

// Use 'as const' object instead of Enum for better type erasure compatibility
export const MotionLawType = {
  POLY5: "Polynomial5",
  MODIFIED_SINE: "ModifiedSine",
  CYCLOIDAL: "Cycloidal",
  CONSTANT_VELOCITY: "ConstantVelocity"
} as const;
export type MotionLawType = typeof MotionLawType[keyof typeof MotionLawType];

export const CoordinateMode = {
  ABSOLUTE: "Absolute",
  RELATIVE: "Relative"
} as const;
export type CoordinateMode = typeof CoordinateMode[keyof typeof CoordinateMode];

export const ReferenceType = {
  MASTER: "Master",
  TIME: "Time"
} as const;
export type ReferenceType = typeof ReferenceType[keyof typeof ReferenceType];

export interface Segment {
  id: string;
  motionLaw: MotionLawType;
  
  // New Fields for Industrial Motion Kernel
  coordinateMode: CoordinateMode;
  referenceType: ReferenceType;
  
  // Flexible Value Fields (Interpretation depends on Mode/Ref)
  masterVal: number; // Absolute End OR Delta OR Duration
  slaveVal: number;  // Absolute End OR Delta
  
  // Display Helpers (Optional, calculated by backend but maybe useful to track local state)
  // For MVP editing, we just use masterVal/slaveVal directly
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

export interface CalculationResponse {
  points: CamPoint[];
  max_velocity: number;
  max_acceleration: number;
  max_jerk: number;
  events: any[];
}
