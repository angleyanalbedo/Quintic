using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Quintic.Wpf.Core.Models;

namespace Quintic.Wpf.Core.Services
{
    public class PlotService
    {
        public PlotModel SPlotModel { get; private set; }
        public PlotModel VAPlotModel { get; private set; }

        private bool _isSyncingAxes = false;

        public PlotService()
        {
            InitializePlots();
        }

        public void ResetAxes()
        {
            SPlotModel.ResetAllAxes();
            VAPlotModel.ResetAllAxes();
            SPlotModel.InvalidatePlot(true);
            VAPlotModel.InvalidatePlot(true);
        }

        public void SetSeriesVisibility(string seriesKey, bool isVisible)
        {
            foreach (var series in VAPlotModel.Series)
            {
                if (series is LineSeries lineSeries && lineSeries.YAxisKey == seriesKey)
                {
                    lineSeries.IsVisible = isVisible;
                }
            }
            VAPlotModel.InvalidatePlot(true);
        }

        public void UpdatePlots(CalculationResponse response)
        {
            if (SPlotModel == null || VAPlotModel == null) return;

            // Update S Series
            var sSeries = SPlotModel.Series[0] as LineSeries;
            if (sSeries != null)
            {
                sSeries.Points.Clear();
                foreach (var p in response.Points)
                {
                    sSeries.Points.Add(new DataPoint(p.Theta, p.S));
                }
            }

            // Update V, A, J Series
            var vSeries = VAPlotModel.Series[0] as LineSeries;
            var aSeries = VAPlotModel.Series[1] as LineSeries;
            var jSeries = VAPlotModel.Series[2] as LineSeries;

            if (vSeries != null) vSeries.Points.Clear();
            if (aSeries != null) aSeries.Points.Clear();
            if (jSeries != null) jSeries.Points.Clear();

            foreach (var p in response.Points)
            {
                if (vSeries != null) vSeries.Points.Add(new DataPoint(p.Theta, p.V));
                if (aSeries != null) aSeries.Points.Add(new DataPoint(p.Theta, p.A));
                if (jSeries != null) jSeries.Points.Add(new DataPoint(p.Theta, p.J));
            }

            SPlotModel.InvalidatePlot(true);
            VAPlotModel.InvalidatePlot(true);
        }

        private void InitializePlots()
        {
            var plotBackground = OxyColor.Parse("#111111");
            var textColor = OxyColor.Parse("#DDDDDD");
            var gridColor = OxyColor.Parse("#222222");
            var tickColor = OxyColor.Parse("#444444");

            // --- S-Curve Plot ---
            SPlotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(40, 10, 20, 30),
                TextColor = textColor
            };

            var sXAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Master Angle (°)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = tickColor,
                TextColor = textColor
            };
            SPlotModel.Axes.Add(sXAxis);

            SPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Position (mm)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = textColor
            });
            
            SPlotModel.Series.Add(new LineSeries
            {
                Title = "Displacement",
                Color = OxyColor.Parse("#3498DB"),
                StrokeThickness = 3,
                MarkerType = MarkerType.None // Performance optimization for high resolution
            });

            // --- V/A/J Plot ---
            VAPlotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(40, 10, 40, 30),
                TextColor = textColor
            };

            var vaXAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Master Angle (°)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineColor = tickColor,
                TextColor = textColor
            };
            VAPlotModel.Axes.Add(vaXAxis);

            // --- Axis Synchronization Logic ---
            // When the user zooms/pans one plot, the other follows instantly.
            sXAxis.AxisChanged += (s, e) =>
            {
                if (_isSyncingAxes) return;
                _isSyncingAxes = true;
                vaXAxis.Zoom(sXAxis.ActualMinimum, sXAxis.ActualMaximum);
                VAPlotModel.InvalidatePlot(false);
                _isSyncingAxes = false;
            };

            vaXAxis.AxisChanged += (s, e) =>
            {
                if (_isSyncingAxes) return;
                _isSyncingAxes = true;
                sXAxis.Zoom(vaXAxis.ActualMinimum, vaXAxis.ActualMaximum);
                SPlotModel.InvalidatePlot(false);
                _isSyncingAxes = false;
            };

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "V",
                Title = "Velocity",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#2ECC71")
            });

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Key = "A",
                Title = "Accel",
                MajorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#E67E22")
            });

             VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Key = "J",
                Title = "Jerk",
                PositionTier = 1,
                MajorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#9B59B6")
            });

            VAPlotModel.Series.Add(new LineSeries { Title = "Velocity", Color = OxyColor.Parse("#2ECC71"), StrokeThickness = 2, YAxisKey = "V" });
            VAPlotModel.Series.Add(new LineSeries { Title = "Acceleration", Color = OxyColor.Parse("#E67E22"), StrokeThickness = 2, YAxisKey = "A" });
            VAPlotModel.Series.Add(new LineSeries { Title = "Jerk", Color = OxyColor.Parse("#9B59B6"), StrokeThickness = 1.5, YAxisKey = "J", LineStyle = LineStyle.Dash });
        }
    }
}
