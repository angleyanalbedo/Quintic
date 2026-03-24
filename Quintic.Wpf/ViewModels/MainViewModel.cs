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
            // S-Curve Plot (Displacement)
            SPlotModel = new PlotModel { Title = "Displacement (S)", PlotAreaBorderColor = OxyColors.Gray, TextColor = OxyColors.White };
            SPlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Master (deg)", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, TicklineColor = OxyColors.White, AxislineColor = OxyColors.White });
            SPlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Slave (mm)", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, TicklineColor = OxyColors.White, AxislineColor = OxyColors.White });
            
            var sSeries = new LineSeries { Title = "Position", Color = OxyColor.Parse("#3498DB"), StrokeThickness = 3 };
            sSeries.Points.Add(new DataPoint(0, 0));
            sSeries.Points.Add(new DataPoint(90, 50));
            sSeries.Points.Add(new DataPoint(180, 50));
            sSeries.Points.Add(new DataPoint(360, 0));
            SPlotModel.Series.Add(sSeries);

            // V/A Plot (Velocity/Acceleration)
            VAPlotModel = new PlotModel { Title = "Velocity (V) & Acceleration (A)", PlotAreaBorderColor = OxyColors.Gray, TextColor = OxyColors.White };
            VAPlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Master (deg)", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, TicklineColor = OxyColors.White, AxislineColor = OxyColors.White });
            VAPlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Velocity", Key = "V", TextColor = OxyColor.Parse("#2ECC71"), TicklineColor = OxyColors.White, AxislineColor = OxyColors.White });
            VAPlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right, Title = "Acceleration", Key = "A", TextColor = OxyColor.Parse("#E67E22"), TicklineColor = OxyColors.White, AxislineColor = OxyColors.White });

            var vSeries = new LineSeries { Title = "Velocity", Color = OxyColor.Parse("#2ECC71"), YAxisKey = "V" };
            // Dummy data
            for (int i = 0; i <= 360; i+=10) vSeries.Points.Add(new DataPoint(i, System.Math.Sin(i * System.Math.PI / 180)));
            
            var aSeries = new LineSeries { Title = "Acceleration", Color = OxyColor.Parse("#E67E22"), YAxisKey = "A", LineStyle = LineStyle.Dash };
            // Dummy data
            for (int i = 0; i <= 360; i += 10) aSeries.Points.Add(new DataPoint(i, System.Math.Cos(i * System.Math.PI / 180)));

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
