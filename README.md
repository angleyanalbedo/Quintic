# Quintic (图洛) - Industrial Cam Editor

Quintic 是一款符合 **VDI 2143** 标准、基于 **C# + WPF** 开发的通用工业电子凸轮曲线设计平台。

本项目专为追求极致性能与用户体验的自动化工程师打造，提供数据密集型、专业深色模式 UI，对标 **Beckhoff TwinCAT 3**、**CODESYS SoftMotion** 和 **Siemens TIA Portal** 等顶级商业软件的交互体验。

## 核心特性

*   **黄金 UI 布局**：经典的“三段式”设计——数据网格、S曲线画布、V/A导数视图，完美兼顾宏观节奏与微观精度。
*   **高性能绘图**：基于 `OxyPlot` 实现工业级曲线渲染，支持数十万点流畅缩放与平移。
*   **双向绑定**：所见即所得的交互体验，拖拽曲线与修改表格数据实时同步。
*   **专业视觉**：自带现代深色主题（炭灰背景 + 电光蓝/橙色高亮），缓解工程师长时间工作的视觉疲劳。

## 技术栈

*   **框架**：.NET 6+ / WPF
*   **数学核心**：`MathNet.Numerics`
*   **绘图引擎**：`OxyPlot.Wpf`
*   **架构模式**：MVVM (Model-View-ViewModel)

## 快速开始

1.  打开 `Quintic.sln` 解决方案。
2.  还原 NuGet 包：
    ```bash
    dotnet restore
    ```
3.  启动 `Quintic.Wpf` 项目。

## 目录结构

*   `Quintic.Wpf/`
    *   `Themes/`: 存放 XAML 资源字典（深色主题定义）。
    *   `ViewModels/`: MVVM 业务逻辑与数据绑定。
    *   `Views/`: UI 界面文件。
    *   `Models/`: 核心数据模型（Segment 定义等）。
    *   `Controls/`: 自定义 UI 控件。

---
*Built with ❤️ for Motion Control Engineers.*
