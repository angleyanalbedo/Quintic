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

### 3.2 画布控制点拖拽 (Canvas Dragging)
- **Goal:** 在 S 曲线的分段交界处渲染可交互的“控制点”。
- **Implementation:** 实现鼠标拖动控制点时，反向更新 DataGrid 里的数值，并实时扭动 V/A/J 曲线。

### 3.3 全局物理极限与超限报警 (Limit Validation)
- **Goal:** 增加全局配置区：输入伺服电机的物理极限（$V_{max}, A_{max}$）。
- **Visual Feedback:** 越界视觉反馈：当算出的曲线超限时，导数图表中的违规区域高亮为红色，同时 DataGrid 对应行闪烁警告。

### 4.1 高密度点表导出 (Raw Profile Export) ✅
- **Goal:** 支持一键导出 .csv 文件，适配基础单片机、自研运动板卡或查表型 PLC 的执行需求。
- **Status:** 已完成 (CsvExporter).

### 4.2 结构化文本生成 (ST Code Generation)
- **Goal:** 结合 PLC ST 分析与生成工具经验，将凸轮数据直接翻译成符合 IEC 61131-3 标准的 ARRAY 代码块。

### 4.3 宿主 IDE 集成接口 (API for Host IDE)
- **Goal:** 将整个 WPF 窗体封装为通用控件（UserControl）或类库。
- **Integration:** 提供对外接口，确保未来只需一行代码，即可将该凸轮编辑器无缝内嵌至 CASS 工业自动化 IDE 中。

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
