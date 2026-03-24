using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Quintic.Wpf.Models;

namespace Quintic.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public PlotModel SPlotModel { get; private set; }
        public PlotModel VAPlotModel { get; private set; }
        public ObservableCollection<Segment> Segments { get; set; }

        public MainViewModel()
        {
            Segments = new ObservableCollection<Segment>
            {
                new Segment { MasterStart = 0, MasterEnd = 90, SlaveStart = 0, SlaveEnd = 50, MotionLaw = "Quintic" },
                new Segment { MasterStart = 90, MasterEnd = 180, SlaveStart = 50, SlaveEnd = 50, MotionLaw = "Dwell" },
                new Segment { MasterStart = 180, MasterEnd = 360, SlaveStart = 50, SlaveEnd = 0, MotionLaw = "Quintic" }
            };

            InitializePlots();
        }

        private void InitializePlots()
        {
            var plotBackground = OxyColor.Parse("#111111");
            var textColor = OxyColor.Parse("#DDDDDD");
            var gridColor = OxyColor.Parse("#222222"); // 极淡的网格线
            var tickColor = OxyColor.Parse("#444444");

            // --- S-Curve Plot (Displacement) ---
            SPlotModel = new PlotModel
            {
                Title = "", // 移除默认标题，用 UI 的 TextBlock 替代
                PlotAreaBorderThickness = new OxyThickness(0), // 移除边框，融入背景
                PlotMargins = new OxyThickness(40, 10, 20, 30), // 调整边距
                TextColor = textColor
            };

            // X Axis
            SPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Master Angle (°)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None, // 极简风格，不要 Tick
                AxislineStyle = LineStyle.Solid,
                AxislineColor = tickColor,
                TextColor = textColor
            });

            // Y Axis
            SPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Position (mm)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None, // 不显示 Y 轴竖线，只靠网格
                TextColor = textColor
            });
            
            var sSeries = new LineSeries
            {
                Title = "Displacement",
                Color = OxyColor.Parse("#3498DB"), // 电光蓝
                StrokeThickness = 3,
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColor.Parse("#3498DB"),
                MarkerFill = OxyColors.Black,
                MarkerStrokeThickness = 2
                // Smooth = true // 启用平滑
            };
            sSeries.Points.Add(new DataPoint(0, 0));
            sSeries.Points.Add(new DataPoint(90, 50));
            sSeries.Points.Add(new DataPoint(180, 50));
            sSeries.Points.Add(new DataPoint(360, 0));
            SPlotModel.Series.Add(sSeries);

            // --- V/A Plot (Velocity/Acceleration) ---
            VAPlotModel = new PlotModel
            {
                Title = "",
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(40, 10, 40, 30), // 右侧留空给 A 轴
                TextColor = textColor
            };

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Master Angle (°)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineColor = tickColor,
                TextColor = textColor
            });

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "V",
                Title = "Velocity",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#2ECC71") // 绿色文字
            });

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Key = "A",
                Title = "Accel",
                MajorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#E67E22") // 橙色文字
            });

            var vSeries = new LineSeries
            {
                Title = "Velocity",
                Color = OxyColor.Parse("#2ECC71"),
                StrokeThickness = 2,
                YAxisKey = "V"
                // Smooth = true
            };
            // Dummy data
            for (int i = 0; i <= 360; i+=5) vSeries.Points.Add(new DataPoint(i, System.Math.Sin(i * System.Math.PI / 180) * 100));
            
            var aSeries = new LineSeries
            {
                Title = "Acceleration",
                Color = OxyColor.Parse("#E67E22"),
                StrokeThickness = 2,
                YAxisKey = "A",
                LineStyle = LineStyle.Solid // 实线更现代
                // Smooth = true
            };
            // Dummy data
            for (int i = 0; i <= 360; i += 5) aSeries.Points.Add(new DataPoint(i, System.Math.Cos(i * System.Math.PI / 180) * 500));

            VAPlotModel.Series.Add(vSeries);
            VAPlotModel.Series.Add(aSeries);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
