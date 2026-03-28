# Quintic Architecture & Roadmap

This document outlines the architectural evolution of the Quintic Cam Editor, from its inception as an MVP to a vision of becoming a Tier-1 Industrial Motion Tool comparable to Siemens TIA CamTool or Beckhoff TwinCAT Cam Design.

## ✅ Phase 1: The Agile MVP (Completed)
**Focus:** Core Data Flow (Input -> Math -> Viz -> CSV)
- **Tech:** React + TypeScript (Hardcoded Math).
- **Scope:** Single R-R Segment, Polynomial 5th Order only.
- **Outcome:** Validated the "Web-Based Editor" concept.

## ✅ Phase 2: Horizontal Expansion (Completed)
**Focus:** Scalability & Math Foundation
- **Tech:** Python (FastAPI + NumPy) Backend.
- **Scope:** Multi-Segment Support, Vectorized Kernels.
- **Outcome:** 50x performance boost, clean `api/kernels/services` architecture.

### 2.1 Editor Essentials (Completed)
- **Project Persistence:** ✅ Save/Load project state (`.json` / `.quintic`) to preserve segments, config, and view settings.
- **Undo/Redo Stack:** ✅ Robust history management with snapshot optimization.

## ✅ Phase 3: Industrial Motion Kernel (Completed)
**Focus:** VDI 2143 Advanced Definitions
- **Tech:** Compiler Pattern (Definition vs. Execution).
- **Scope:** 
    - **Dual Reference Domains:** Time (s) vs. Master (deg).
    - **Coordinate Modes:** Relative (Delta) vs. Absolute.
    - **Law Library:** Polynomial 5/7, Cycloidal, Modified Sine/Trapezoid, Simple Sine, 7-Segment, Gutman.
    - **Analytics:** Real-time Characteristic Values ($V_{max}, A_{max}, J_{max}$).
- **Outcome:** A flexible kernel capable of handling complex machine logic (e.g., "Dwell for 100ms then Move 50mm").

---

## 🔮 Phase 4: Interaction & Integration (Completed)

### 4.1 Canvas Dragging (Interactive Control Points) ✅
- **Goal:** Render interactive "Control Points" at segment junctions on the S-curve.
- **Status:** Completed.
- **Implementation:** Implemented mouse drag events in `PlotService` to update `Segment` data in real-time and recalculate V/A/J curves.

### 4.2 Canvas Point Addition (Split Segment) ✅
- **Goal:** Allow users to add new points directly on the curve to split segments.
- **Status:** Completed.
- **Implementation:** Implemented `Ctrl + LeftClick` on the canvas to calculate the exact split point and insert a new segment while maintaining continuity.

### 4.3 Global Physical Limits & Alarm (Limit Validation) ✅
- **Goal:** Global configuration for servo motor physical limits ($V_{max}, A_{max}$).
- **Status:** Completed.
- **Features:**
    - **Charts:** Highlight violating regions in red on V/A plots (`RectangleAnnotation`).
    - **Grid:** Highlight corresponding rows in the DataGrid with red background.

### 4.4 Raw Profile Export (High Density CSV) ✅
- **Goal:** One-click export to .csv for microcontrollers or table-based PLCs.
- **Status:** Completed (`CsvExporter`).

### 4.5 Structured Text Generation (ST Code) ✅
- **Goal:** Generate IEC 61131-3 compliant ARRAY code blocks directly from cam data.
- **Status:** Completed (`StCodeGenerator`).

### 4.6 Host IDE Integration API ✅
- **Goal:** Encapsulate the WPF window as a generic UserControl or Class Library.
- **Integration:** Provide an external API to embed the cam editor into the CASS Industrial Automation IDE with a single line of code.
- **Status:** Completed (`Quintic.Core` library separation).

### 4.7 Discrete Logic Tracks (Cam Switch / PLS) ✅
- **Goal:** Microsecond-level synchronization of Motion and IO.
- **UI:** Multi-channel tracks below the main chart for defining IO On/Off intervals via drag-and-drop.
- **Status:** Completed.
- **Features:**
    - **Hysteresis Compensation:** Prevent signal jitter at boundary points.
    - **Export:** Serialize switch points into Bitmasks or PLC-compatible arrays.

---

## 🔮 Phase 5: Advanced Math (Future)
**Goal:** Match the interpolation capabilities of mature commercial tools (Siemens/Rexroth).

### 5.1 Boundary Value Solver (Automatic Continuity) ✅
- **Problem:** Currently, users must manually ensure $V_{end}$ of Segment A matches $V_{start}$ of Segment B.
- **Solution:** Implemented forward propagation of $V$ and $A$ boundary conditions in `CamCalculator`.

### 5.2 Spline Interpolation ✅
- **Solution:** Implement **B-Splines (Cubic/Quintic)** for smooth curve fitting through point clouds.
- **Status:** Completed (`BSpline`).

### 5.3 Kinematic Analysis Dashboard (In Progress)
**Goal:** Quantified "Crash Prevention" report, benchmarking Siemens SIZER or Beckhoff TC3 Motion Designer.

#### Phase 1: Physics Engine & Core Math (Partially Completed)
- **Multi-Inertia Modeling:** ✅ Basic inertia summation ($J_{total}$) implemented.
- **Friction Models:** ✅ Coulomb friction ($T_c$) support added.
- **Key Metrics:** ✅ RMS Acceleration, Peak Jerk, Peak Torque, and Power Prediction implemented in `KinematicAnalysisViewModel`.

#### Phase 2: Visual KPI Cards & Real-time Alerts (Completed)
- **Health View:** ✅ RMS Thermal Load Bar & Peak Torque Gauge implemented with color-coded warnings.
- **Diagnostics Log:** ✅ Auto-detect mechanical resonance risks (High Jerk) and drive capacity issues.

#### Phase 3: Advanced Plotting & Simulation (Completed)
- **T-N Curve Overlays:** ✅ Scatter plot of Torque vs. Speed with S1 (Continuous) and S3 (Intermittent) operation boundaries implemented.
- **Dynamic Toggles:** ✅ Checkboxes for Torque, Power, and Regenerative Energy curves.
- **Sync Cursor:** ✅ Crosshair showing instantaneous Torque/Power on the position curve.

#### Phase 4: Automated Reporting (Completed)
- **PDF Generation:** ✅ One-click export of "Crash Prevention" reports with KPI summaries, danger zone screenshots, and sizing recommendations.

---

## 🔮 Phase 6: Application Intelligence
**Goal:** Decouple "Motion Design" from "Machine Process".

### 6.1 Application Wizards
- **Flying Shear (飞锯)**
- **Rotary Knife (旋切)**
- **Cross Sealer (横封)**

### 6.2 Motor Sizing
- Integrate Motor & Load Models (Inertia, Friction, Torque Curves).

---

## 🔮 Phase 7: Real-time Signal Processing (New)
**Goal:** Handle real-world sensor noise and mechanical imperfections (Soft-PLC features).

### 7.1 Input Signal Conditioning
- **Feature:** First-Order Low-Pass Filter for Master Encoder.
- **Config:** User-configurable `FilterConstant` (alpha) and `LagCompensation`.

### 7.2 Dynamic Error Correction
- **Feature:** Dead-band management for phase synchronization.
- **Logic:** Ignore errors < `Threshold` (e.g., 0.05mm) to prevent motor oscillation.

### 7.3 Soft-Motion S-Curve Injection
- **Feature:** Superimpose correction moves using S-Curve profiles instead of step changes to prevent "Overcurrent".
