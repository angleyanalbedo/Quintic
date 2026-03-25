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

### 2.1 Editor Essentials (Ongoing)
- **Project Persistence:** Save/Load project state (`.json` / `.quintic`) to preserve segments, config, and view settings.
- **Undo/Redo Stack:** Essential for trial-and-error design.

## ✅ Phase 3: Industrial Motion Kernel (Completed)
**Focus:** VDI 2143 Advanced Definitions
- **Tech:** Compiler Pattern (Definition vs. Execution).
- **Scope:** 
    - **Dual Reference Domains:** Time (s) vs. Master (deg).
    - **Coordinate Modes:** Relative (Delta) vs. Absolute.
    - **Law Library:** Polynomial 5, Cycloidal, Modified Sine/Trapezoid.
    - **Analytics:** Real-time Characteristic Values ($V_{max}, A_{max}, J_{max}$).
- **Outcome:** A flexible kernel capable of handling complex machine logic (e.g., "Dwell for 100ms then Move 50mm").

---

## 🔮 Phase 4: The "Smoothness" Engine (Next Priority)
**Goal:** Match the interpolation capabilities of mature commercial tools (Siemens/Rexroth). Currently, Quintic relies on explicit analytical segments. Real-world applications often require smooth transitions between arbitrary points.

### 4.1 Boundary Value Solver (Automatic Continuity)
- **Problem:** Currently, users must manually ensure $V_{end}$ of Segment A matches $V_{start}$ of Segment B.
- **Solution:** Implement a **Global Solver** (using `scipy.optimize` or `scipy.linalg`).
    - User defines points $(X, Y)$ and constraints (e.g., $V=0$ at start/end).
    - System automatically calculates the required boundary velocities/accelerations for all intermediate points to ensure $C^2$ (Acceleration) continuity.

### 4.2 Spline Interpolation
- **Problem:** Analytical laws (Poly5, ModSine) are rigid. They pass exactly through start/end points but can have "overshoot" between them if points are close.
- **Solution:** Implement **B-Splines (Cubic/Quintic)**.
    - Allow users to input a "Cloud of Points" (e.g., from a measured profile).
    - Fit a smooth curve through these points with adjustable "Tension" parameters (similar to CAD tools).

### 4.3 Transition Handling (Rounding)
- **Feature:** "Blend" or "Round" corners between linear segments automatically (e.g., adding a circular or polynomial fillet between two straight lines).

### 4.4 Interactive Design (The "Editor" Experience)
- **Control Points:** Render draggable handles (white/blue hollow circles) at segment junctions $(M_{end}, S_{end})$.
- **Real-time Feedback:** Dragging a handle instantly updates the $S$ curve and recalculates $V, A, J$ derivatives.
- **Boundary Constraints UI:** Expand the segment table to allow explicit input of $V_{start}, V_{end}, A_{start}, A_{end}$ (currently hidden/assumed zero).

---

## 🔮 Phase 5: Application Intelligence
**Goal:** Decouple "Motion Design" from "Machine Process". Engineers shouldn't calculate cam angles; they should input product dimensions.

### 5.1 Application Wizards (The "Money" Features)
- **Flying Shear (飞锯):** Input: Product Length, Line Speed, Cut Time. -> Output: Synchronous Cam Profile.
- **Rotary Knife (旋切):** Input: Knife Radius, Product Length. -> Output: Sync + Retract Profile.
- **Cross Sealer (横封):** Input: Pack Depth, Dwell Time. -> Output: Sealing Profile.

### 5.2 Kinematics & Dynamics Check
- **Physical Limits Alarm:**
    - User sets global limits (e.g., $V_{max} = 1000$ mm/s, $A_{max} = 5g$).
    - **Visual Feedback:** Draw red horizontal limit lines on $V/A$ plots. Highlight areas exceeding limits in red.
    - **Table Feedback:** Flag specific segments in the data grid that violate these limits.
- **Motor Sizing:** Integrate Motor & Load Models.
    - Input: Inertia ($J_{load}$), Friction, Motor Torque Curve ($T_{motor}$).
    - Output: **"Torque Utilization"** chart. Warn user if the cam profile exceeds the motor's physical capability.

---

## 🔮 Phase 6: Ecosystem Integration
**Goal:** Direct integration with PLC environments (Siemens, Rockwell, Beckhoff).

### 6.1 IEC 61131-3 Code Generation
- Instead of "Dumb CSV", generate "Smart Code".
- **Structured Text (ST):** Generate a function block that calculates the profile on the fly (for memory-constrained PLCs).
- **Data Blocks (DB):** Generate Siemens S7 source files (`.awl` / `.scl`) or TIA Portal Openness XML.
- **L5X:** Generate Rockwell Add-On Instructions (AOI).

### 6.2 Online Monitoring (Digital Twin)
- Connect to the PLC via OPC UA / Modbus.
- Overlay the **"Actual Position"** (Encoder Feedback) on top of the **"Command Position"** (Quintic Profile) to diagnose tracking errors in real-time.
