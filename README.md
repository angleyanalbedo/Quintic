# Quintic - Industrial Cam Editor

Quintic is a universal industrial electronic cam profile design platform compliant with the **VDI 2143** standard, developed using **C# + WPF**.

This project is built for automation engineers who demand extreme performance and user experience. It provides a data-intensive, professional dark mode UI, benchmarking the interactive experience of top-tier commercial software such as **Beckhoff TwinCAT 3**, **CODESYS SoftMotion**, and **Siemens TIA Portal**.

## Core Features

*   **Golden UI Layout**: Classic "Left List + Right Canvas" layout, maximizing widescreen usage, perfectly balancing macroscopic rhythm and microscopic precision.
*   **VDI 2143 Standard Kernel**: Built-in industrial standard motion laws, generating Position (S), Velocity (V), Acceleration (A), and Jerk (J) curves via a high-precision mathematical kernel.
    *   **Polynomial 5**: The most common Rest-in-Rest curve.
    *   **Cycloidal**: Suitable for high-speed, low-vibration applications.
    *   **Modified Trapezoid**: Finite jerk, extremely smooth.
    *   **Constant Velocity / Dwell**: Basic linear and dwell segments.
*   **High-Performance Plotting**: Industrial-grade curve rendering based on `OxyPlot`, supporting smooth zooming and panning of hundreds of thousands of points.
*   **Real-time Compilation**: WYSIWYG interactive experience; modifying table data triggers millisecond-level recalculation and instant curve refresh.
*   **CSV Export**: One-click export of high-precision point tables, supporting direct import into mainstream motion controllers like Siemens, Beckhoff, and Omron.
*   **Professional Visuals**: Comes with a modern dark theme (charcoal background + electric blue/orange highlights) to reduce visual fatigue for engineers working long hours.

## Tech Stack

*   **Framework**: .NET 6+ / WPF
*   **Plotting Engine**: `OxyPlot.Wpf`
*   **Architecture Pattern**: MVVM (Model-View-ViewModel) + Command Pattern
*   **Math Core**: C# Native Implementation (Ported from Python NumPy kernels)

## Quick Start

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
