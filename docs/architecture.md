# Industrial Motion Kernel Architecture (Phase 3)

This document outlines the **VDI 2143 Compliant Motion Kernel (Advanced)** architecture for the Quintic project. This design shifts from a simple "Point Table" to a sophisticated **Motion Profile Compiler** architecture, capable of handling relative coordinates, time-based definitions, and complex boundary conditions.

## 1. Core Philosophy: Definition vs. Compilation

To support industrial flexibility (like "FlexProfile"), we decouple **User Intent** from **Mathematical Execution**.

- **Profile Definition (The "Source Code"):**
  - Flexible, relative, and potentially ambiguous.
  - Examples: "Move 50mm relative to last position", "Dwell for 100ms".
  - Defined by the User / Wizard.

- **Profile Compilation (The "Compiler"):**
  - Resolves all ambiguities.
  - Converts Time -> Master Degrees.
  - Converts Relative -> Absolute Coordinates.
  - Validates continuity and physical limits.
  - Produces a **Normalized Execution Profile**.

- **Math Kernel (The "CPU"):**
  - Dumb but fast.
  - Only understands Absolute Coordinates and Normalized Time ($0..1$).
  - Calculates $S, V, A, J$ using pure math functions (e.g., Polynomial 5th Order).

---

## 2. Key Concepts & Data Structures

### 2.1 Reference Systems (Dual Domain)
The kernel supports switching the reference domain for any motion segment.

- **Master Absolute (`MasterAbsolute`):** Standard cam definition. Segment ends at a specific master angle (e.g., $180^\circ$).
- **Master Relative (`MasterRelative`):** Segment duration is a delta from the previous end (e.g., $+90^\circ$).
- **Time Duration (`TimeDuration`):** Segment duration is defined in seconds/milliseconds. The compiler uses the global `MasterVelocity` to convert this to master degrees.

### 2.2 Coordinate Modes
- **Absolute (`Absolute`):** The Slave Axis moves to a specific position (e.g., $100mm$).
- **Relative (`Relative`):** The Slave Axis moves by a delta distance (e.g., $+20mm$).

### 2.3 Execution Modes
- **One Shot (`OneShot`):** Profile runs once. Velocity must be 0 at start and end.
- **Cyclic (`Cyclic`):** Profile repeats. End position/velocity must match Start position/velocity.
- **Cyclic Appending (`CyclicAppending`):** (Future) The end position of cycle $N$ becomes the start position of cycle $N+1$ (e.g., continuous feeding).

---

## 3. Data Schema (JSON Structure)

The backend API accepts a `Project` object.

```json
{
  "config": {
    "master_velocity": 60.0, // RPM (Reference for Time <-> Master conversion)
    "resolution": 360,
    "execution_mode": "OneShot", // or "Cyclic"
    "units_master": "deg",
    "units_slave": "mm"
  },
  "segments": [
    {
      "id": "seg_1",
      "motion_law": "Polynomial5",
      "reference_type": "Master", // or "Time"
      "coordinate_mode": "Absolute", // or "Relative"
      "master_val": 90.0, // Absolute Position
      "slave_val": 50.0   // Absolute Position
    },
    {
      "id": "seg_2",
      "motion_law": "Polynomial5",
      "reference_type": "Time",
      "coordinate_mode": "Relative",
      "master_val": 0.5,  // Duration in Seconds (Compiler converts to Deg)
      "slave_val": 0.0    // Relative move (Dwell)
    }
  ],
  "events": [
    {
      "id": "evt_1",
      "trigger_master_pos": 45.0,
      "action": "SetBit:CutEnable"
    }
  ]
}
```

---

## 4. Processing Pipeline

### Step 1: Resolution (Compiler)
The `CamCalculator._resolve_coordinates` function iterates through the segments:

1.  **Initialize:** `CurrentMaster = 0`, `CurrentSlave = 0`.
2.  **Iterate Segments:**
    - If `TimeDuration`: Calculate `DeltaMaster = Duration * Speed`.
    - If `Relative`: `Target = Current + Value`.
    - If `Absolute`: `Target = Value`.
3.  **Store Computed Values:** Populate `computed_master_start/end` and `computed_slave_start/end` in the segment object.

### Step 2: Validation
- Check for negative durations.
- Check for master position overlaps.
- (Future) Check against `PhysicalLimits` (Max Velocity/Accel).

### Step 3: Kernel Execution
- The `CamCalculator` iterates through the **Resolved Segments**.
- It instantiates the appropriate Math Kernel (e.g., `Polynomial5`).
- It generates the discrete point table $(S, V, A, J)$.
- It stitches the segments together, handling boundary points to avoid duplication.

---

## 5. Future Extensions

1.  **Application Wizards:**
    - Flying Shear Wizard: Generates the Project JSON based on product length and cut speed.
    - Rotary Knife Wizard: Generates synchronization profiles.
    - These wizards sit *above* the API and output standard JSON.

2.  **Hardware Export:**
    - The "Normalized Execution Profile" (Result of Step 3) can be exported to:
      - CSV (Generic)
      - PLC Data Block (Siemens S7)
      - L5K/L5X (Allen-Bradley)
      - XML (Beckhoff)

3.  **Multi-Axis Synchronization:**
    - Defining `Slave` as another `Master` for a second profile (Cascading Cams).
