# Quintic - Industrial Cam Editor

Quintic is a universal industrial electronic cam profile design platform compliant with the **VDI 2143** standard, developed using **C# + WPF**.

This project is built for automation engineers who demand extreme performance and user experience. It provides a data-intensive, professional dark mode UI, benchmarking the interactive experience of top-tier commercial software such as **Beckhoff TwinCAT 3**, **CODESYS SoftMotion**, and **Siemens TIA Portal**.

![Quintic Cam Editor Screenshot](https://raw.githubusercontent.com/angleyanalbedo/Quintic/master/docs/images/screenshot.png)

## Core Features

*   **Golden UI Layout**: Classic "Left List + Right Canvas" layout, maximizing widescreen usage, perfectly balancing macroscopic rhythm and microscopic precision.
*   **VDI 2143 Standard Kernel**: Built-in industrial standard motion laws, generating Position (S), Velocity (V), Acceleration (A), and Jerk (J) curves via a high-precision mathematical kernel.
    *   **Polynomial 5**: The most common Rest-in-Rest curve.
    *   **Cycloidal**: Suitable for high-speed, low-vibration applications.
    *   **Modified Trapezoid**: Finite jerk, extremely smooth.
    *   **B-Spline**: Cubic spline interpolation for smooth curve fitting through arbitrary points.
    *   **Constant Velocity / Dwell**: Basic linear and dwell segments.
*   **High-Performance Plotting**: Industrial-grade curve rendering based on `OxyPlot`, supporting smooth zooming and panning of hundreds of thousands of points.
*   **Real-time Compilation**: WYSIWYG interactive experience; modifying table data triggers millisecond-level recalculation and instant curve refresh.
*   **Interactive Design**: Drag control points directly on the canvas to intuitively shape the motion profile.
*   **Safety First**: Global physical limit validation ($V_{max}, A_{max}$) with real-time visual alarms on charts and data grids.
*   **History Management**: Full Undo/Redo support for safe experimentation.
*   **CSV Export**: One-click export of high-precision point tables, supporting direct import into mainstream motion controllers like Siemens, Beckhoff, and Omron.
*   **ST Code Generation**: Generate IEC 61131-3 compliant Structured Text (ST) arrays for direct PLC integration.
*   **Discrete Logic Tracks**: Define multi-channel IO switch points synchronized with motion, supporting hysteresis compensation.
*   **PDF Reporting**: One-click generation of professional Kinematic Analysis & Crash Prevention reports.
*   **Host Integration API**: Available as a NuGet package (`Quintic.Core`) for embedding the editor into custom HMI or Industrial IDEs.
*   **Professional Visuals**: Comes with a modern dark theme (charcoal background + electric blue/orange highlights) to reduce visual fatigue for engineers working long hours.

## Tech Stack

*   **Framework**: .NET 6+ / WPF
*   **Plotting Engine**: `OxyPlot.Wpf`
*   **Architecture Pattern**: MVVM (Model-View-ViewModel) + Command Pattern
*   **Math Core**: C# Native Implementation (Ported from Python NumPy kernels)

## Installation & Integration

### 1. Standalone Application
For end-users, simply download the latest executable (`.exe`) from the **[GitHub Releases](https://github.com/angleyanalbedo/Quintic/releases)** page. No installation required.

### 2. WPF Integration (NuGet)
For developers integrating the editor into an HMI or Industrial IDE:

```bash
Install-Package Quintic.Core
```

Add the namespace and control to your XAML:

```xml
<Window ...
        xmlns:q="clr-namespace:Quintic.Wpf.Views;assembly=Quintic.Core">
    
    <!-- Embed the full editor -->
    <q:CamEditorView />
    
</Window>
```

## Basic Usage

*   **Segment Table**: Use the `MOTION PROFILE` tab's data grid to configure motion laws (Poly5, Cycloidal, etc.) and target coordinates manually.
*   **Interactive Canvas**:
    *   **Add Point**: Hold **`Ctrl + Left Click`** anywhere on the curve to split the segment and insert a new control point.
    *   **Edit Point**: Drag existing control points to adjust the profile in real-time.
*   **Logic Tracks**: Use the `LOGIC TRACKS` tab to manage digital output channels. Add tracks and define precise switch degree intervals synchronized with the master position.

## Developer Setup (Source Code)

1.  Open the `Quintic.sln` solution.
2.  Restore NuGet packages:
    ```bash
    dotnet restore
    ```
3.  Start the `Quintic.Wpf` project.

## Directory Structure

*   `Quintic.Wpf/`
    *   `Core/`: Core business logic
        *   `Kernels/`: Mathematical motion law implementations (Poly5, Cycloidal, etc.)
        *   `Services/`: Cam compiler and calculation services
        *   `Models/`: Core data models
    *   `Themes/`: XAML resource dictionaries (Dark theme definitions).
    *   `ViewModels/`: MVVM business logic and data binding.
    *   `Views/`: UI interface files.

---
*Built with ❤️ for Motion Control Engineers.*
