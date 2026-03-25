# Quintic (图洛) - Industrial Cam Editor

Quintic 是一款符合 **VDI 2143** 标准、基于 **C# + WPF** 开发的通用工业电子凸轮曲线设计平台。

本项目专为追求极致性能与用户体验的自动化工程师打造，提供数据密集型、专业深色模式 UI，对标 **Beckhoff TwinCAT 3**、**CODESYS SoftMotion** 和 **Siemens TIA Portal** 等顶级商业软件的交互体验。

## 核心特性

*   **黄金 UI 布局**：经典的“左侧列表 + 右侧画布”布局，最大化利用宽屏空间，完美兼顾宏观节奏与微观精度。
*   **VDI 2143 标准内核**：内置多种工业标准运动规律，通过高精度数学内核生成位置 (S)、速度 (V)、加速度 (A) 和加加速度 (Jerk) 曲线。
    *   **Polynomial 5 (五次多项式)**: 最常用的 Rest-in-Rest 曲线。
    *   **Cycloidal (摆线)**: 适合高速低振动应用。
    *   **Modified Trapezoid (修正梯形)**: 有限冲击，极为平滑。
    *   **Constant Velocity / Dwell**: 基础直线与停滞段。
*   **高性能绘图**：基于 `OxyPlot` 实现工业级曲线渲染，支持数十万点流畅缩放与平移。
*   **实时编译**：所见即所得的交互体验，修改表格数据毫秒级重算，曲线即时刷新。
*   **CSV 导出**：一键导出高精度点表，支持直接导入至 Siemens、Beckhoff、Omron 等主流运动控制器。
*   **专业视觉**：自带现代深色主题（炭灰背景 + 电光蓝/橙色高亮），缓解工程师长时间工作的视觉疲劳。

## 技术栈

*   **框架**：.NET 6+ / WPF
*   **绘图引擎**：`OxyPlot.Wpf`
*   **架构模式**：MVVM (Model-View-ViewModel) + Command Pattern
*   **数学核心**：C# Native Implementation (Ported from Python NumPy kernels)

## 快速开始

1.  打开 `Quintic.sln` 解决方案。
2.  还原 NuGet 包：
    ```bash
    dotnet restore
    ```
3.  启动 `Quintic.Wpf` 项目。

## 目录结构

*   `Quintic.Wpf/`
    *   `Core/`: 核心业务逻辑
        *   `Kernels/`: 数学运动规律实现 (Poly5, Cycloidal, etc.)
        *   `Services/`: 凸轮编译器与计算服务
        *   `Models/`: 核心数据模型
    *   `Themes/`: 存放 XAML 资源字典（深色主题定义）。
    *   `ViewModels/`: MVVM 业务逻辑与数据绑定。
    *   `Views/`: UI 界面文件。

---
*Built with ❤️ for Motion Control Engineers.*
