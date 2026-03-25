# Quintic Architecture: From Python MVP to C# WPF Professional Tool

## 1. Overview
Quintic has evolved from a web-based MVP (Python/React) into a high-performance desktop application (C#/WPF) targeting industrial automation professionals. This document captures the architectural decisions and migration path.

## 2. The Python MVP (Legacy)
**Goal:** Validated the core mathematical kernel and concept of "VDI 2143 Cam Editor".

### Tech Stack
- **Backend:** Python 3.11 + FastAPI + NumPy (Vectorized Math).
- **Frontend:** React + TypeScript + Recharts.
- **Data Flow:** JSON (REST API) -> NumPy Calculation -> JSON Response.

### Key Components
1.  **Kernel (quintic/kernels/*.py)**:
    - `Polynomial5`: Standard 5th-order polynomial ($s = 10\tau^3 - 15\tau^4 + 6\tau^5$).
    - `Cycloidal`: Bestehorn sinusoid for high-speed low-vibration.
    - `ModifiedTrapezoid`: 7th-order approximation for limited jerk.
2.  **Compiler (quintic/services/calculator.py)**:
    - Resolves "Relative" coordinates to "Absolute".
    - Converts "Time Duration" to "Master Angle" based on global velocity.
    - Stitches segments together (End $V_{prev} \to$ Start $V_{next}$).

## 3. The C# WPF Professional Tool (Current)
**Goal:** High-performance, low-latency, native Windows integration (COM/OLE for Excel later), and DirectX/Hardware acceleration potential for massive datasets.

### Tech Stack
- **Framework:** .NET 6+ / WPF (Windows Presentation Foundation).
- **UI Pattern:** MVVM (Model-View-ViewModel) + Command Pattern.
- **Math Library:** Ported Python Kernels to C# + `MathNet.Numerics` (future solver).
- **Visualization:** `OxyPlot.Wpf` (Direct2D/Hardware Rendering).

### Architecture Layers

#### Layer 1: Core (Pure Logic)
*Namespace: `Quintic.Wpf.Core`*
- **Models:** `Segment`, `ProjectConfig`, `CamPoint`.
    - **Smart Segments:** `INotifyPropertyChanged` built-in. Changing `MasterEnd` triggers recalculation immediately.
- **Kernels:** `IMotionKernel` interface.
    - `Polynomial5.cs`
    - `Cycloidal.cs`
    - `ModifiedTrapezoid.cs`
    - `ConstantVelocity.cs`
    - `Dwell.cs`
- **Services:**
    - `CamCalculator`: Static stateless service. Takes `List<Segment>` and returns `CalculationResponse`.
    - `CsvExporter`: Generates industrial standard CSV points.

#### Layer 2: ViewModel (Application State)
*Namespace: `Quintic.Wpf.ViewModels`*
- **MainViewModel:**
    - Holds the `ObservableCollection<Segment>`.
    - Subscribes to `CollectionChanged` and `PropertyChanged` events.
    - **Debouncing:** (Future optimization) Throttles recalculation requests for very fast typing.
    - **OxyPlot Models:** Manages `PlotModel` instances for `S` and `V/A/J` charts.

#### Layer 3: View (Presentation)
*Namespace: `Quintic.Wpf.Views`*
- **MainWindow.xaml:**
    - **Left Sidebar:** `DataGrid` for precision input.
    - **Right Canvas:** `PlotView` for macro visualization.
    - **Theme:** `DarkTheme.xaml` (Dictionary) for professional "Dark Mode" look.

## 4. Migration Strategy (Python -> C#)
| Component | Python (Old) | C# (New) | Status |
| :--- | :--- | :--- | :--- |
| **Math Kernel** | NumPy (Vectorized) | C# Double Precision Loop | ✅ Done |
| **Compiler** | `resolve_coordinates` | `CamCalculator.ResolveCoordinates` | ✅ Done |
| **API** | FastAPI (REST) | In-Memory Direct Call | ✅ Done |
| **UI** | React (DOM) | WPF (DirectX/GDI+) | ✅ Done |
| **Plotting** | Recharts (SVG) | OxyPlot (Canvas) | ✅ Done |

## 5. Future: The "Solver" Engine
The next architectural leap is implementing the **Global Continuity Solver**.
- **Problem:** Currently, segments calculate velocity based *only* on their own boundary conditions (0 by default).
- **Solution:** A system of linear equations ($Ax = B$) to solve for boundary velocities $v_i$ such that acceleration is continuous across all $i$ points.
- **Implementation:** Will use `MathNet.Numerics.LinearAlgebra` to solve the tridiagonal matrix (Spline Interpolation).

---
*Documented on: March 25, 2026*
