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
- **Project Persistence:** ✅ Save/Load project state (`.json` / `.quintic`) to preserve segments, config, and view settings.
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

## 🔮 Phase 4: Interaction & Integration (Next Priority)

### 4.1 Canvas Dragging (Interactive Control Points) ✅
- **Goal:** Render interactive "Control Points" at segment junctions on the S-curve.
- **Status:** Completed.
- **Implementation:** Implemented mouse drag events in `PlotService` to update `Segment` data in real-time and recalculate V/A/J curves.

### 4.2 Global Physical Limits & Alarm (Limit Validation) ✅
- **Goal:** Global configuration for servo motor physical limits ($V_{max}, A_{max}$).
- **Status:** Completed.
- **Features:**
    - **Charts:** Highlight violating regions in red on V/A plots (`RectangleAnnotation`).
    - **Grid:** Highlight corresponding rows in the DataGrid with red background.

### 4.3 Raw Profile Export (High Density CSV) ✅
- **Goal:** One-click export to .csv for microcontrollers or table-based PLCs.
- **Status:** Completed (`CsvExporter`).

### 4.4 Structured Text Generation (ST Code)
- **Goal:** Generate IEC 61131-3 compliant ARRAY code blocks directly from cam data.

### 4.5 Host IDE Integration API
- **Goal:** Encapsulate the WPF window as a generic UserControl or Class Library.
- **Integration:** Provide an external API to embed the cam editor into the CASS Industrial Automation IDE with a single line of code.

---

## 🔮 Phase 5: Advanced Math (Future)
**Goal:** Match the interpolation capabilities of mature commercial tools (Siemens/Rexroth).

### 5.1 Boundary Value Solver (Automatic Continuity)
- **Problem:** Currently, users must manually ensure $V_{end}$ of Segment A matches $V_{start}$ of Segment B.
- **Solution:** Implement a **Global Solver** (using `scipy.optimize` or `scipy.linalg`).

### 5.2 Spline Interpolation
- **Solution:** Implement **B-Splines (Cubic/Quintic)** for smooth curve fitting through point clouds.

---

## 🔮 Phase 6: Application Intelligence
**Goal:** Decouple "Motion Design" from "Machine Process".

### 6.1 Application Wizards
- **Flying Shear (飞锯)**
- **Rotary Knife (旋切)**
- **Cross Sealer (横封)**

### 6.2 Motor Sizing
- Integrate Motor & Load Models (Inertia, Friction, Torque Curves).
