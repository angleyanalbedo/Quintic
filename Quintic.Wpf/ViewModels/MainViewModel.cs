using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;

using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Services;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public PlotModel SPlotModel { get; private set; }
        public PlotModel VAPlotModel { get; private set; }
        public ObservableCollection<Segment> Segments { get; set; }
        public ProjectConfig Config { get; set; }
        public ICommand ExportCsvCommand { get; private set; }

        private CalculationResponse _lastCalculation;

        public MainViewModel()
        {
            Config = new ProjectConfig { Resolution = 1000 };

            Segments = new ObservableCollection<Segment>
            {
                new Segment { MasterVal = 90, SlaveVal = 50, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 180, SlaveVal = 50, MotionLaw = MotionLawType.Dwell, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 360, SlaveVal = 0, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute }
            };

            // Event Subscriptions
            Segments.CollectionChanged += OnSegmentsCollectionChanged;
            foreach (var seg in Segments)
            {
                seg.PropertyChanged += OnSegmentPropertyChanged;
            }

            ExportCsvCommand = new RelayCommand(ExecuteExportCsv);

            InitializePlots();
            Recalculate();
        }

        private void ExecuteExportCsv(object obj)
        {
            if (_lastCalculation == null || _lastCalculation.Points.Count == 0) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv|Text File (*.txt)|*.txt",
                DefaultExt = ".csv",
                FileName = "CamProfile.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                CsvExporter.Export(saveFileDialog.FileName, _lastCalculation);
            }
        }

        private void OnSegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Segment item in e.NewItems)
                    item.PropertyChanged += OnSegmentPropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (Segment item in e.OldItems)
                    item.PropertyChanged -= OnSegmentPropertyChanged;
            }
            Recalculate();
        }

        private void OnSegmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("Computed")) return;
            Recalculate();
        }

        private void Recalculate()
        {
            _lastCalculation = CamCalculator.CalculateProject(Segments.ToList(), Config);
            UpdatePlots(_lastCalculation);

            double currentM = 0;
            double currentS = 0;
            foreach (var seg in Segments)
            {
                seg.ComputedMasterStart = currentM;
                seg.ComputedSlaveStart = currentS;
                
                if (seg.CoordinateMode == CoordinateMode.Absolute)
                {
                    seg.ComputedMasterEnd = seg.MasterVal;
                    seg.ComputedSlaveEnd = seg.SlaveVal;
                }
                else
                {
                    seg.ComputedMasterEnd = currentM + seg.MasterVal;
                    seg.ComputedSlaveEnd = currentS + seg.SlaveVal;
                }
                
                currentM = seg.ComputedMasterEnd ?? 0;
                currentS = seg.ComputedSlaveEnd ?? 0;
            }
        }

        private void UpdatePlots(CalculationResponse response)
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

            SPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Master Angle (°)",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = gridColor,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = tickColor,
                TextColor = textColor
            });

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
