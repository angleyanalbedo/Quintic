# Release Notes

## v0.5.0 - Advanced Motion Kernels
**Date:** 2026-03-26

### 🚀 New Features
- **Advanced Motion Laws**: Added comprehensive support for industrial standard curves:
  - **Polynomial 7 (4-5-6-7)**: Provides smoother motion with zero jerk at boundaries compared to Poly5.
  - **Simple Sine**: Basic harmonic motion for simple mechanisms.
  - **Gutman (Freydenstein)**: Sinus-combination curve designed for vibration suppression.
  - **7-Segment S-Curve**: Standard trapezoidal acceleration profile used in servo positioning.
- **Automatic Continuity**: `CamCalculator` now automatically propagates end-velocity and end-acceleration to the next segment (enabling C2 continuity for Poly5).

### 🛠 Improvements
- **Architecture**: Refactored Motion Kernels to use a unified `BaseMotionKernel` and `IMotionKernel` interface.
- **Stability**: Enforced zero-displacement logic for `Dwell` segments to prevent user configuration errors.
- **Namespace**: Unified all Core logic under `Quintic.Wpf.Core` namespace.
