using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using Quintic.Wpf.Core.Models;

namespace Quintic.Wpf.Core.Services
{
    public class PlotService
    {
        public PlotModel SPlotModel { get; private set; }
        public PlotModel VAPlotModel { get; private set; }

        public event System.Action<int, double, double> PointDragged;
        public event System.Action<double, double> CanvasCtrlClicked;
        public event System.Action DragStarted;
        public event System.Action DragFinished;

        private bool _isSyncingAxes = false;
        private int _dragIndex = -1;

        private LineAnnotation _sCursor;
        private LineAnnotation _vaCursor;

        public PlotService()
        {
            InitializePlots();
            
            // Hook up interaction events
            SPlotModel.MouseDown += OnSPlotMouseDown;
            SPlotModel.MouseMove += OnSPlotMouseMove;
            SPlotModel.MouseUp += OnSPlotMouseUp;

            VAPlotModel.MouseMove += OnVAPlotMouseMove;
            
            // Also listen to mouse leave to hide cursor? Or just let it stay. Let's let it stay.
        }

        private void OnSPlotMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left) return;

            // Handle Ctrl + Click to Add Point (Split Segment)
            if (e.ModifierKeys.HasFlag(OxyModifierKeys.Control))
            {
                var xAxis = SPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
                var yAxis = SPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Left);

                if (xAxis != null && yAxis != null)
                {
                    // Check if we are clicking too close to an existing point (optional, but good for UX)
                    var series = SPlotModel.Series.OfType<ScatterSeries>().FirstOrDefault();
                    if (series != null)
                    {
                        var nearest = series.GetNearestPoint(e.Position, false);
                        if (nearest != null && nearest.Position.DistanceTo(e.Position) < 15)
                        {
                            return; // Ignore if clicking on an existing control point
                        }
                    }

                    var x = xAxis.InverseTransform(e.Position.X);
                    var y = yAxis.InverseTransform(e.Position.Y);
                    CanvasCtrlClicked?.Invoke(x, y);
                    e.Handled = true;
                    return;
                }
            }

            var seriesDrag = SPlotModel.Series.OfType<ScatterSeries>().FirstOrDefault();
            if (seriesDrag == null) return;

            var nearestDrag = seriesDrag.GetNearestPoint(e.Position, false);
            if (nearestDrag != null && nearestDrag.Position.DistanceTo(e.Position) < 15)
            {
                _dragIndex = (int)nearestDrag.Index;
                DragStarted?.Invoke();
                e.Handled = true;
                SPlotModel.InvalidatePlot(false);
            }
        }

        private void OnSPlotMouseMove(object sender, OxyMouseEventArgs e)
        {
            if (_dragIndex >= 0)
            {
                var series = SPlotModel.Series.OfType<ScatterSeries>().FirstOrDefault();
                if (series != null)
                {
                    var dataPoint = series.InverseTransform(e.Position);
                    PointDragged?.Invoke(_dragIndex, dataPoint.X, dataPoint.Y);
                    e.Handled = true;
                }
            }
            else
            {
                var xAxis = SPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
                if (xAxis != null)
                {
                    double x = xAxis.InverseTransform(e.Position.X);
                    UpdateCursors(x);
                }
            }
        }

        private void OnVAPlotMouseMove(object sender, OxyMouseEventArgs e)
        {
            var xAxis = VAPlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null)
            {
                double x = xAxis.InverseTransform(e.Position.X);
                UpdateCursors(x);
            }
        }

        private void UpdateCursors(double x)
        {
            if (_sCursor == null || _vaCursor == null) return;

            _sCursor.X = x;
            _vaCursor.X = x;

            if (_lastResponse != null && _lastResponse.Points.Count > 0)
            {
                // Find nearest point
                var point = _lastResponse.Points.OrderBy(p => System.Math.Abs(p.Theta - x)).FirstOrDefault();
                if (point != null)
                {
                    double jTotal = _lastConfig != null ? _lastConfig.LoadInertia + _lastConfig.MotorInertia : 0.015;
                    double friction = _lastConfig != null ? _lastConfig.FrictionTorque : 0.1;
                    
                    double torque = (jTotal * point.A) + (friction * System.Math.Sign(point.V));
                    double power = torque * point.V;

                    _sCursor.Text = $"Θ: {point.Theta:F1}°\nS: {point.S:F2}\nT: {torque:F2} Nm\nP: {power:F2} W";
                    _vaCursor.Text = $"Θ: {point.Theta:F1}°\nV: {point.V:F2}\nA: {point.A:F2}\nT: {torque:F2} Nm\nP: {power:F2} W";
                }
            }

            SPlotModel.InvalidatePlot(false);
            VAPlotModel.InvalidatePlot(false);
        }

        private void OnSPlotMouseUp(object sender, OxyMouseEventArgs e)
        {
            if (_dragIndex != -1)
            {
                _dragIndex = -1;
                DragFinished?.Invoke();
            }
        }

        public void ResetAxes()
        {
            SPlotModel.ResetAllAxes();
            VAPlotModel.ResetAllAxes();
            SPlotModel.InvalidatePlot(true);
            VAPlotModel.InvalidatePlot(true);
        }

        public void SetSeriesVisibilityByTitle(string title, bool isVisible)
        {
            var series = VAPlotModel.Series.FirstOrDefault(s => s.Title == title);
            if (series != null)
            {
                series.IsVisible = isVisible;
            }

            // Also manage axis visibility. If all series on an axis are hidden, hide the axis.
            if (series is XYAxisSeries xySeries)
            {
                var axis = VAPlotModel.Axes.FirstOrDefault(a => a.Key == xySeries.YAxisKey);
                if (axis != null)
                {
                    bool anyVisible = VAPlotModel.Series.OfType<XYAxisSeries>()
                        .Any(s => s.YAxisKey == axis.Key && s.IsVisible);
                    axis.IsAxisVisible = anyVisible;
                }
            }

            VAPlotModel.InvalidatePlot(true);
        }

        public void UpdateLimitLines(double maxV, double maxA)
        {
            // Remove old limit lines only (keep rectangles if any, or clear all? Let's clear lines)
            var oldLines = VAPlotModel.Annotations.Where(a => a is LineAnnotation).ToList();
            foreach (var a in oldLines) VAPlotModel.Annotations.Remove(a);

            // Velocity Limits (+/-)
            var vLimitColor = OxyColor.FromAColor(60, OxyColors.Red);
            VAPlotModel.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Horizontal, Y = maxV, Color = vLimitColor, StrokeThickness = 1, LineStyle = LineStyle.Dash, YAxisKey = "V", Text = "V Max" });
            VAPlotModel.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Horizontal, Y = -maxV, Color = vLimitColor, StrokeThickness = 1, LineStyle = LineStyle.Dash, YAxisKey = "V" });

            // Acceleration Limits (+/-)
            var aLimitColor = OxyColor.FromAColor(60, OxyColors.OrangeRed);
            VAPlotModel.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Horizontal, Y = maxA, Color = aLimitColor, StrokeThickness = 1, LineStyle = LineStyle.Dash, YAxisKey = "A", Text = "A Max" });
            VAPlotModel.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Horizontal, Y = -maxA, Color = aLimitColor, StrokeThickness = 1, LineStyle = LineStyle.Dash, YAxisKey = "A" });

            VAPlotModel.InvalidatePlot(true);
        }

        public void HighlightLimitViolations(CalculationResponse response, double maxV, double maxA)
        {
            // Clear old violation rectangles
            var oldRects = VAPlotModel.Annotations.Where(a => a is RectangleAnnotation && (string)a.Tag == "Violation").ToList();
            foreach (var a in oldRects) VAPlotModel.Annotations.Remove(a);

            if (response == null || response.Points == null) return;

            void AddViolation(double startX, double endX, string axisKey)
            {
                VAPlotModel.Annotations.Add(new RectangleAnnotation
                {
                    MinimumX = startX,
                    MaximumX = endX,
                    Fill = OxyColor.FromAColor(40, OxyColors.Red),
                    YAxisKey = axisKey,
                    Tag = "Violation",
                    Layer = AnnotationLayer.BelowSeries
                });
            }

            // Scan Velocity
            double? startV = null;
            foreach (var p in response.Points)
            {
                bool isOver = System.Math.Abs(p.V) > maxV;
                if (isOver && startV == null) startV = p.Theta;
                else if (!isOver && startV != null)
                {
                    AddViolation(startV.Value, p.Theta, "V");
                    startV = null;
                }
            }
            if (startV != null) AddViolation(startV.Value, response.Points.Last().Theta, "V");

            // Scan Acceleration
            double? startA = null;
            foreach (var p in response.Points)
            {
                bool isOver = System.Math.Abs(p.A) > maxA;
                if (isOver && startA == null) startA = p.Theta;
                else if (!isOver && startA != null)
                {
                    AddViolation(startA.Value, p.Theta, "A");
                    startA = null;
                }
            }
            if (startA != null) AddViolation(startA.Value, response.Points.Last().Theta, "A");

            VAPlotModel.InvalidatePlot(true);
        }

        private CalculationResponse _lastResponse;
        private ProjectConfig _lastConfig;

        public void UpdatePlots(CalculationResponse response, ProjectConfig config, IEnumerable<Segment> segments = null)
        {
            _lastResponse = response;
            _lastConfig = config;

            if (SPlotModel == null || VAPlotModel == null) return;

            // Update Control Points (Interactive Design Prep)
            if (segments != null)
            {
                var cpSeries = SPlotModel.Series.OfType<ScatterSeries>().FirstOrDefault();
                if (cpSeries == null)
                {
                    cpSeries = new ScatterSeries
                    {
                        Title = "Control Points",
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 5,
                        MarkerFill = OxyColor.Parse("#1A1A1A"),
                        MarkerStroke = OxyColor.Parse("#3498DB"),
                        MarkerStrokeThickness = 2
                    };
                    SPlotModel.Series.Add(cpSeries);
                }

                cpSeries.Points.Clear();
                foreach (var seg in segments)
                {
                    if (seg.ComputedMasterEnd.HasValue && seg.ComputedSlaveEnd.HasValue)
                    {
                        cpSeries.Points.Add(new ScatterPoint(seg.ComputedMasterEnd.Value, seg.ComputedSlaveEnd.Value));
                    }
                }
            }

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

            // Update V, A, J, T, P, Regen Series
            var vSeries = VAPlotModel.Series.FirstOrDefault(s => s.Title == "Velocity") as LineSeries;
            var aSeries = VAPlotModel.Series.FirstOrDefault(s => s.Title == "Acceleration") as LineSeries;
            var jSeries = VAPlotModel.Series.FirstOrDefault(s => s.Title == "Jerk") as LineSeries;
            var tSeries = VAPlotModel.Series.FirstOrDefault(s => s.Title == "Torque") as LineSeries;
            var pSeries = VAPlotModel.Series.FirstOrDefault(s => s.Title == "Power") as LineSeries;
            var rSeries = VAPlotModel.Series.FirstOrDefault(s => s.Title == "Regen Energy") as AreaSeries;

            if (vSeries != null) vSeries.Points.Clear();
            if (aSeries != null) aSeries.Points.Clear();
            if (jSeries != null) jSeries.Points.Clear();
            if (tSeries != null) tSeries.Points.Clear();
            if (pSeries != null) pSeries.Points.Clear();
            if (rSeries != null) { rSeries.Points.Clear(); rSeries.Points2.Clear(); }

            double jTotal = config != null ? config.LoadInertia + config.MotorInertia : 0.015;
            double friction = config != null ? config.FrictionTorque : 0.1;

            foreach (var p in response.Points)
            {
                if (vSeries != null) vSeries.Points.Add(new DataPoint(p.Theta, p.V));
                if (aSeries != null) aSeries.Points.Add(new DataPoint(p.Theta, p.A));
                if (jSeries != null) jSeries.Points.Add(new DataPoint(p.Theta, p.J));

                double torque = (jTotal * p.A) + (friction * System.Math.Sign(p.V));
                double power = torque * p.V;

                if (tSeries != null) tSeries.Points.Add(new DataPoint(p.Theta, torque));
                if (pSeries != null) pSeries.Points.Add(new DataPoint(p.Theta, power));
                
                if (rSeries != null)
                {
                    double regenPower = power < 0 ? power : 0;
                    rSeries.Points.Add(new DataPoint(p.Theta, regenPower));
                    rSeries.Points2.Add(new DataPoint(p.Theta, 0));
                }
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

            _sCursor = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1,
                TextColor = textColor
            };
            SPlotModel.Annotations.Add(_sCursor);

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
                
                // Auto-adjust Y axes on VAPlot when X is zoomed
                foreach (var axis in VAPlotModel.Axes.Where(a => a.Position != AxisPosition.Bottom))
                {
                    axis.Reset();
                }

                VAPlotModel.InvalidatePlot(false);
                _isSyncingAxes = false;
            };

            vaXAxis.AxisChanged += (s, e) =>
            {
                if (_isSyncingAxes) return;
                _isSyncingAxes = true;
                sXAxis.Zoom(vaXAxis.ActualMinimum, vaXAxis.ActualMaximum);
                
                // Auto-adjust Y axes on SPlot when X is zoomed
                foreach (var axis in SPlotModel.Axes.Where(a => a.Position != AxisPosition.Bottom))
                {
                    axis.Reset();
                }
                
                // Auto-adjust Y axes on VAPlot itself during horizontal pan/zoom
                if (e.ChangeType == AxisChangeTypes.Zoom || e.ChangeType == AxisChangeTypes.Pan)
                {
                    foreach (var axis in VAPlotModel.Axes.Where(a => a.Position != AxisPosition.Bottom))
                    {
                        axis.Reset();
                    }
                }

                SPlotModel.InvalidatePlot(false);
                VAPlotModel.InvalidatePlot(false);
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

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Key = "T",
                Title = "Torque",
                PositionTier = 2,
                MajorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#E74C3C") // Red-ish
            });

            VAPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Key = "P",
                Title = "Power",
                PositionTier = 3,
                MajorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                TextColor = OxyColor.Parse("#F1C40F") // Yellow
            });

            VAPlotModel.Series.Add(new LineSeries { Title = "Velocity", Color = OxyColor.Parse("#2ECC71"), StrokeThickness = 2, YAxisKey = "V" });
            VAPlotModel.Series.Add(new LineSeries { Title = "Acceleration", Color = OxyColor.Parse("#E67E22"), StrokeThickness = 2, YAxisKey = "A" });
            VAPlotModel.Series.Add(new LineSeries { Title = "Jerk", Color = OxyColor.Parse("#9B59B6"), StrokeThickness = 1.5, YAxisKey = "J", LineStyle = LineStyle.Dash });
            
            VAPlotModel.Series.Add(new LineSeries { Title = "Torque", Color = OxyColor.Parse("#E74C3C"), StrokeThickness = 2, YAxisKey = "T", IsVisible = false });
            VAPlotModel.Series.Add(new LineSeries { Title = "Power", Color = OxyColor.Parse("#F1C40F"), StrokeThickness = 2, YAxisKey = "P", IsVisible = false });
            
            VAPlotModel.Series.Add(new AreaSeries 
            { 
                Title = "Regen Energy", 
                Color = OxyColor.Parse("#8E44AD"), 
                Fill = OxyColor.FromAColor(100, OxyColors.Purple), 
                StrokeThickness = 0, 
                YAxisKey = "P", 
                IsVisible = false 
            });

            _vaCursor = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1,
                TextColor = textColor,
                TextHorizontalAlignment = HorizontalAlignment.Left
            };
            VAPlotModel.Annotations.Add(_vaCursor);
        }
    }
}
