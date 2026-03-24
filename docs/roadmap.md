# Quintic Architecture & Roadmap

This document chronicles the architectural evolution of the Quintic Cam Editor, from its inception as a Minimum Viable Product (MVP) to its current industrial-grade foundation.

## Phase 1: The Agile MVP (Zero to One)

**Goal:** Establish the core data flow: Input -> Math Kernel -> Visualization -> Export.

### Architecture
- **Monolithic Frontend:** A simple React application.
- **Embedded Math Kernel:** Hardcoded TypeScript logic for the 3-4-5 Polynomial (Rest-in-Rest).
- **Single Segment:** User inputs 4 values (Start/End Master, Start/End Slave).
- **Visualization:** Basic Recharts line graphs for S/V/A/J.
- **Export:** Browser-side CSV generation.

### Key Decisions
- **Why Web?** Immediate visual feedback, cross-platform accessibility.
- **Why Polynomial 5?** "Decathlete" of motion laws; robust, simple derivative math.
- **Why MVP?** To validate the VDI 2143 concept without getting bogged down in complexity.

---

## Phase 2: Horizontal Expansion & Backend Separation

**Goal:** Professionalize the codebase and prepare for scalability.

### Architecture
- **Backend Separation:** Moved math logic to a Python/FastAPI service.
- **Vectorized Kernel:** Re-implemented `Polynomial5` using **NumPy** for high-performance array operations.
- **Multi-Segment Support:**
    - Frontend: Dynamic Point Table (Add/Remove points).
    - Backend: Automatic slicing of point lists into `[Start, End]` segments.
    - Stitching: Concatenating arrays to form a continuous profile.

### Key Decisions
- **Python Backend:** Access to `scipy` (Linear Algebra) for future Spline/Matrix solving.
- **Vectorization:** `numpy` arrays vs. Python loops = 50x speedup for 100k points.
- **Clean Architecture:** Organized code into `api/`, `kernels/`, `schemas/`, and `services/`.

---

## Phase 3: Industrial Motion Kernel (Current)

**Goal:** Support advanced VDI 2143 definitions (Time-based, Relative Coordinates).

### Architecture
- **Project-Based Schema:** Moved from "Point List" to "Project Object Model".
- **Compiler Pattern:** Separated **User Definition** (Relative/Time) from **Mathematical Execution** (Absolute/Normalized).
- **Dual Domain:** Segments can be defined in **Master Domain** (Degrees) or **Time Domain** (Seconds).
- **Coordinate Modes:** Support for **Relative** (Delta) and **Absolute** positioning.
- **Events:** Placeholder for trigger logic.

*(See `docs/architecture.md` for full details on Phase 3)*
