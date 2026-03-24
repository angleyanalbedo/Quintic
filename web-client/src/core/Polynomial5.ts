export interface CamProfile {
  s: number; // Displacement
  v: number; // Velocity
  a: number; // Acceleration
  j: number; // Jerk
  theta: number; // Master axis position
}

export class Polynomial5 {
  masterStart: number;
  masterEnd: number;
  slaveStart: number;
  slaveEnd: number;

  constructor(
    masterStart: number,
    masterEnd: number,
    slaveStart: number,
    slaveEnd: number
  ) {
    this.masterStart = masterStart;
    this.masterEnd = masterEnd;
    this.slaveStart = slaveStart;
    this.slaveEnd = slaveEnd;
  }

  // Calculate the profile at a specific master position theta
  calculate(theta: number): CamProfile {
    // 1. Normalize theta to tau [0, 1]
    const beta = this.masterEnd - this.masterStart;
    
    // Handle out of bounds (clamping) or return 0 for R-R outside range
    if (theta < this.masterStart) return { s: this.slaveStart, v: 0, a: 0, j: 0, theta };
    if (theta > this.masterEnd) return { s: this.slaveEnd, v: 0, a: 0, j: 0, theta };

    const tau = (theta - this.masterStart) / beta;
    const h = this.slaveEnd - this.slaveStart;

    // 2. 5th Order Polynomial Coefficients (Rest-in-Rest)
    // s(tau) = 10*tau^3 - 15*tau^4 + 6*tau^5
    
    // Displacement
    const s_norm = 10 * Math.pow(tau, 3) - 15 * Math.pow(tau, 4) + 6 * Math.pow(tau, 5);
    const s = this.slaveStart + h * s_norm;

    // Velocity (First Derivative)
    // v_norm = 30*tau^2 - 60*tau^3 + 30*tau^4
    // v = (h / beta) * v_norm
    const v_norm = 30 * Math.pow(tau, 2) - 60 * Math.pow(tau, 3) + 30 * Math.pow(tau, 4);
    const v = (h / beta) * v_norm;

    // Acceleration (Second Derivative)
    // a_norm = 60*tau - 180*tau^2 + 120*tau^3
    // a = (h / beta^2) * a_norm
    const a_norm = 60 * tau - 180 * Math.pow(tau, 2) + 120 * Math.pow(tau, 3);
    const a = (h / Math.pow(beta, 2)) * a_norm;

    // Jerk (Third Derivative)
    // j_norm = 60 - 360*tau + 360*tau^2
    // j = (h / beta^3) * j_norm
    const j_norm = 60 - 360 * tau + 360 * Math.pow(tau, 2);
    const j = (h / Math.pow(beta, 3)) * j_norm;

    return { s, v, a, j, theta };
  }

  // Generate discrete points for the entire range
  generateTable(resolution: number = 100): CamProfile[] {
    const data: CamProfile[] = [];
    const step = (this.masterEnd - this.masterStart) / (resolution - 1);

    for (let i = 0; i < resolution; i++) {
        const theta = this.masterStart + i * step;
        data.push(this.calculate(theta));
    }
    return data;
  }
}
