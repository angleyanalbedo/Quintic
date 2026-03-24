# Quintic (图洛)

**Industrial-Grade VDI 2143 Cam Profile Generator**

Quintic is a modern, high-performance motion design tool built for automation engineers. It implements the rigorous VDI 2143 standard for cam design, featuring a robust Python mathematical kernel and an interactive React frontend.

Unlike simple "point-to-point" editors, Quintic uses a sophisticated **Motion Compiler Architecture** that supports relative coordinates, time-based definitions, and complex motion laws.

---

## 🚀 Key Features

### 1. Advanced Motion Definitions
- **Dual Reference Domains:** Define segments by **Master Position** (degrees) OR **Time Duration** (seconds). The compiler automatically converts time to master angles based on global velocity.
- **Coordinate Modes:** Support for **Absolute** positioning and **Relative** (Delta) movements.
- **Flexible Segments:** Mix and match absolute/relative and time/master definitions in a single profile.

### 2. Comprehensive Motion Law Library
- **Polynomial 5th Order (3-4-5):** The "Decathlete" — balanced S-curve acceleration (Rest-in-Rest).
- **Cycloidal (Bestehorn):** Pure sinusoidal acceleration for high-speed, low-vibration applications.
- **Modified Sine / Trapezoid:** Low jerk, high load capacity curves approximated via Polynomial 7.

### 3. Real-Time Analytics
- **Instant Visualization:** Interactive charts for Displacement ($S$), Velocity ($V$), Acceleration ($A$), and Jerk ($J$).
- **Characteristic Dashboard:** Real-time calculation of $V_{max}$, $A_{max}$, and $J_{max}$ to evaluate motor sizing and mechanical stress.

### 4. Industrial Integration
- **Export:** Generate CSV point tables ready for import into Siemens, Rockwell, Beckhoff, or standard Servo Drives.
- **Clean Architecture:** "Definition vs. Compilation" pattern ensures robust handling of complex logic.

---

## 🏗️ Architecture

- **Frontend:** React 19 + TypeScript + Vite + Recharts + TailwindCSS.
- **Backend:** Python 3.11 + FastAPI + NumPy (Vectorized Kernels).
- **Core Logic:**
    1.  **User Definition:** Accepts flexible inputs (e.g., "Move 50mm in 100ms").
    2.  **Compiler:** Resolves relative/time inputs into normalized absolute coordinates.
    3.  **Math Kernel:** Executes high-precision vectorized polynomials to generate the profile.

---

## ⚡ Getting Started

### Prerequisites
- Node.js & npm
- Python 3.11+
- [uv](https://github.com/astral-sh/uv) (Recommended Python package manager)

### 1. Start the Backend (Python)

```bash
cd quintic
# Install dependencies and run server
uv run uvicorn main:app --reload
```
*The API will be available at `http://localhost:8000`.*

### 2. Start the Frontend (React)

```bash
cd web-client
npm install
npm run dev
```
*Open `http://localhost:5173` in your browser to start designing.*

---

## 📚 Documentation

See the `docs/` folder for detailed architectural decisions:
- [Architecture Overview](docs/architecture.md)
- [Project Roadmap & History](docs/roadmap.md)
