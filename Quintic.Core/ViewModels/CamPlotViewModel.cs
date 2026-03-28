using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Services;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class CamPlotViewModel : INotifyPropertyChanged
    {
        private readonly PlotService _plotService;

        public PlotModel SPlotModel => _plotService.SPlotModel;
        public PlotModel VAPlotModel => _plotService.VAPlotModel;
        public PlotModel TracksPlotModel { get; private set; }

        public event System.Action<int, double, double> PointDragged;
        public event System.Action<double, double> CanvasCtrlClicked;
        public event System.Action DragStarted;
        public event System.Action DragFinished;

        public ICommand ResetViewCommand { get; private set; }

        private void UpdatePlotVisibility()
        {
            _plotService.SetSeriesVisibilityByTitle("Velocity", IsVelocityVisible);
            _plotService.SetSeriesVisibilityByTitle("Acceleration", IsAccelerationVisible);
            _plotService.SetSeriesVisibilityByTitle("Jerk", IsJerkVisible);
            _plotService.SetSeriesVisibilityByTitle("Torque", IsTorqueVisible);
            _plotService.SetSeriesVisibilityByTitle("Power", IsPowerVisible);
            _plotService.SetSeriesVisibilityByTitle("Regen Energy", IsRegenVisible);
        }

        private bool _isVelocityVisible = true;
        public bool IsVelocityVisible
        {
            get => _isVelocityVisible;
            set
            {
                if (_isVelocityVisible != value)
                {
                    _isVelocityVisible = value;
                    UpdatePlotVisibility();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isAccelerationVisible = true;
        public bool IsAccelerationVisible
        {
            get => _isAccelerationVisible;
            set
            {
                if (_isAccelerationVisible != value)
                {
                    _isAccelerationVisible = value;
                    UpdatePlotVisibility();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isJerkVisible = true;
        public bool IsJerkVisible
        {
            get => _isJerkVisible;
            set
            {
                if (_isJerkVisible != value)
                {
                    _isJerkVisible = value;
                    UpdatePlotVisibility();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTorqueVisible = false;
        public bool IsTorqueVisible
        {
            get => _isTorqueVisible;
            set
            {
                if (_isTorqueVisible != value)
                {
                    _isTorqueVisible = value;
                    UpdatePlotVisibility();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isPowerVisible = false;
        public bool IsPowerVisible
        {
            get => _isPowerVisible;
            set
            {
                if (_isPowerVisible != value)
                {
                    _isPowerVisible = value;
                    UpdatePlotVisibility();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRegenVisible = false;
        public bool IsRegenVisible
        {
            get => _isRegenVisible;
            set
            {
                if (_isRegenVisible != value)
                {
                    _isRegenVisible = value;
                    UpdatePlotVisibility();
                    OnPropertyChanged();
                }
            }
        }

        public CamPlotViewModel()
        {
            _plotService = new PlotService();

            // Lock Y-Axis zooming/panning for S-Curve to match V/A/J plots
            foreach (var axis in _plotService.SPlotModel.Axes.Where(a => a.Position == AxisPosition.Left || a.Position == AxisPosition.Right))
            {
                axis.IsZoomEnabled = false;
                axis.IsPanEnabled = false;
            }

            _plotService.PointDragged += (i, m, s) => PointDragged?.Invoke(i, m, s);
            _plotService.CanvasCtrlClicked += (m, s) => CanvasCtrlClicked?.Invoke(m, s);
            _plotService.DragStarted += () => DragStarted?.Invoke();
            _plotService.DragFinished += () => DragFinished?.Invoke();
            ResetViewCommand = new RelayCommand(o => _plotService.ResetAxes());

            InitializeTracksPlot();
        }

        private void InitializeTracksPlot()
        {
            TracksPlotModel = new PlotModel
            {
                PlotMargins = new OxyThickness(40, 5, 20, 30),
                Background = OxyColors.Transparent,
                TextColor = OxyColors.White
            };

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 360,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
                TextColor = OxyColors.White
            };
            TracksPlotModel.Axes.Add(xAxis);

            var yAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                TextColor = OxyColors.White,
                TickStyle = TickStyle.None,
                GapWidth = 0.2
            };
            TracksPlotModel.Axes.Add(yAxis);

            // Sync X-Axis with Main Plot
            var mainXAxis = _plotService.SPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (mainXAxis != null)
            {
                mainXAxis.AxisChanged += (s, e) =>
                {
                    if (xAxis.Minimum != mainXAxis.ActualMinimum || xAxis.Maximum != mainXAxis.ActualMaximum)
                    {
                        xAxis.Zoom(mainXAxis.ActualMinimum, mainXAxis.ActualMaximum);
                        TracksPlotModel.InvalidatePlot(false);
                    }
                };
            }
        }

        public void UpdatePlots(CalculationResponse response, ProjectConfig config, IEnumerable<Segment> segments = null)
        {
            _plotService.UpdatePlots(response, config, segments);
        }

        public void UpdateLogicTracks(IEnumerable<CamTrack> tracks)
        {
            TracksPlotModel.Annotations.Clear();
            
            var trackList = tracks.ToList();
            var yAxis = TracksPlotModel.Axes.OfType<CategoryAxis>().FirstOrDefault();
            if (yAxis != null)
            {
                yAxis.ItemsSource = trackList.Select(t => t.Name).ToList();
                // Ensure axis range covers all tracks
                yAxis.Minimum = -0.5;
                yAxis.Maximum = trackList.Count - 0.5;
            }

            for (int i = 0; i < trackList.Count; i++)
            {
                var track = trackList[i];
                foreach (var sw in track.Switches)
                {
                    var rect = new RectangleAnnotation
                    {
                        MinimumX = sw.OnAngle,
                        MaximumX = sw.OffAngle,
                        MinimumY = i - 0.3,
                        MaximumY = i + 0.3,
                        Fill = OxyColor.FromAColor(150, OxyColors.Orange),
                        Stroke = OxyColors.White,
                        StrokeThickness = 1,
                        Text = $"{sw.OnAngle:F0}-{sw.OffAngle:F0}",
                        TextColor = OxyColors.White
                    };

                    // Simple interaction: Click to see details (Drag implementation requires more complex logic)
                    rect.ToolTip = $"Track: {track.Name}\nOn: {sw.OnAngle:F1}\nOff: {sw.OffAngle:F1}";

                    TracksPlotModel.Annotations.Add(rect);
                }
            }

            TracksPlotModel.InvalidatePlot(true);
        }

        public void UpdateLimits(double maxV, double maxA)
        {
            _plotService.UpdateLimitLines(maxV, maxA);
        }

        public void HighlightViolations(CalculationResponse response, double maxV, double maxA)
        {
            _plotService.HighlightLimitViolations(response, maxV, maxA);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
