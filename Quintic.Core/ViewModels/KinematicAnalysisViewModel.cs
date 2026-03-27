using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Quintic.Wpf.Core.Models;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Annotations;

namespace Quintic.Wpf.ViewModels
{
    public class KinematicAnalysisViewModel : INotifyPropertyChanged
    {
        private CalculationResponse _lastResponse;
        private ProjectConfig _config;

        public ProjectConfig Config
        {
            get => _config;
            set { _config = value; OnPropertyChanged(); RecalculateKinematics(); }
        }

        private double _rmsAcceleration;
        public double RmsAcceleration
        {
            get => _rmsAcceleration;
            set { _rmsAcceleration = value; OnPropertyChanged(); }
        }

        private double _peakJerk;
        public double PeakJerk
        {
            get => _peakJerk;
            set { _peakJerk = value; OnPropertyChanged(); }
        }

        private double _peakTorque;
        public double PeakTorque
        {
            get => _peakTorque;
            set { _peakTorque = value; OnPropertyChanged(); }
        }

        private double _rmsTorque;
        public double RmsTorque
        {
            get => _rmsTorque;
            set { _rmsTorque = value; OnPropertyChanged(); }
        }
        
        private double _peakPower;
        public double PeakPower
        {
            get => _peakPower;
            set { _peakPower = value; OnPropertyChanged(); }
        }

        private PlotModel _torqueSpeedModel;
        public PlotModel TorqueSpeedModel
        {
            get
            {
                // Prevent "PlotModel is already in use" exception when reopening the window
                if (_torqueSpeedModel != null && _torqueSpeedModel.PlotView != null)
                {
                    _torqueSpeedModel = CreateTorqueSpeedModel();
                }
                return _torqueSpeedModel;
            }
            set { _torqueSpeedModel = value; OnPropertyChanged(); }
        }

        // Motor Ratings
        private double _ratedTorque = 2.0;
        public double RatedTorque
        {
            get => _ratedTorque;
            set { _ratedTorque = value; OnPropertyChanged(); RecalculateKinematics(); }
        }

        private double _maxTorque = 6.0;
        public double MaxTorque
        {
            get => _maxTorque;
            set { _maxTorque = value; OnPropertyChanged(); RecalculateKinematics(); }
        }

        // KPI & Alerts
        private double _rmsLoadPercentage;
        public double RmsLoadPercentage
        {
            get => _rmsLoadPercentage;
            set { _rmsLoadPercentage = value; OnPropertyChanged(); }
        }

        private double _peakLoadPercentage;
        public double PeakLoadPercentage
        {
            get => _peakLoadPercentage;
            set { _peakLoadPercentage = value; OnPropertyChanged(); }
        }

        private string _rmsStatusColor = "Green";
        public string RmsStatusColor
        {
            get => _rmsStatusColor;
            set { _rmsStatusColor = value; OnPropertyChanged(); }
        }

        private string _peakStatusColor = "Green";
        public string PeakStatusColor
        {
            get => _peakStatusColor;
            set { _peakStatusColor = value; OnPropertyChanged(); }
        }

        private string _diagnosticsLog;
        public string DiagnosticsLog
        {
            get => _diagnosticsLog;
            set { _diagnosticsLog = value; OnPropertyChanged(); }
        }

        public void Update(CalculationResponse response, ProjectConfig config)
        {
            _lastResponse = response;
            Config = config; // This triggers RecalculateKinematics
        }

        public void RecalculateKinematics()
        {
            if (_lastResponse == null || _lastResponse.Points.Count == 0 || _config == null) return;

            // 1. Peak Jerk
            PeakJerk = _lastResponse.MaxJerk;

            // 2. RMS Acceleration = Sqrt( Sum(a^2) / N )
            double sumSqAcc = _lastResponse.Points.Sum(p => p.A * p.A);
            RmsAcceleration = Math.Sqrt(sumSqAcc / _lastResponse.Points.Count);

            // 3. Torque & Power
            // T = J_total * alpha + T_friction * sign(v)
            // P = T * omega
            
            double jTotal = _config.LoadInertia + _config.MotorInertia;
            double maxT = 0;
            double sumSqT = 0;
            double maxP = 0;

            foreach (var p in _lastResponse.Points)
            {
                // Simplified Physics Model
                double torque = Math.Abs(jTotal * p.A) + _config.FrictionTorque;
                
                if (torque > maxT) maxT = torque;
                sumSqT += torque * torque;
                
                double power = torque * Math.Abs(p.V); 
                if (power > maxP) maxP = power;
            }

            PeakTorque = maxT;
            RmsTorque = Math.Sqrt(sumSqT / _lastResponse.Points.Count);
            PeakPower = maxP;

            TorqueSpeedModel = CreateTorqueSpeedModel();

            // KPI Calculations
            RmsLoadPercentage = (RatedTorque > 0) ? (RmsTorque / RatedTorque) * 100 : 0;
            PeakLoadPercentage = (MaxTorque > 0) ? (PeakTorque / MaxTorque) * 100 : 0;

            // Status Colors
            RmsStatusColor = RmsLoadPercentage > 100 ? "Red" : (RmsLoadPercentage > 80 ? "Orange" : "Green");
            PeakStatusColor = PeakLoadPercentage > 100 ? "Red" : (PeakLoadPercentage > 90 ? "Orange" : "Green");

            // Diagnostics Log
            var logBuilder = new System.Text.StringBuilder();
            if (RmsLoadPercentage > 100) logBuilder.AppendLine("⚠️ Motor Overheating Risk (RMS > Rated)");
            if (PeakLoadPercentage > 100) logBuilder.AppendLine("⛔ Drive Current Limit Exceeded (Peak > Max)");
            else if (PeakLoadPercentage > 90) logBuilder.AppendLine("⚠️ Near Torque Limit");
            
            if (PeakJerk > 1000) logBuilder.AppendLine("⚠️ High Jerk Detected (Check Mechanics)");

            DiagnosticsLog = logBuilder.ToString();
            if (string.IsNullOrEmpty(DiagnosticsLog)) DiagnosticsLog = "System Healthy";
        }

        private PlotModel CreateTorqueSpeedModel()
        {
            var model = new PlotModel { Title = "Torque vs Speed (T-N Curve)" };
            
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Velocity", Unit = "units/s" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Torque", Unit = "Nm" });

            if (_lastResponse == null || _config == null) return model;

            // S1 Limit (Continuous)
            if (RatedTorque > 0)
            {
                var s1Area = new RectangleAnnotation
                {
                    MinimumY = -RatedTorque, MaximumY = RatedTorque,
                    Fill = OxyColor.FromAColor(30, OxyColors.Green),
                    Text = "S1 (Continuous)"
                };
                model.Annotations.Add(s1Area);
            }

            // S3 Limit (Intermittent)
            if (MaxTorque > RatedTorque)
            {
                var s3AreaTop = new RectangleAnnotation
                {
                    MinimumY = RatedTorque, MaximumY = MaxTorque,
                    Fill = OxyColor.FromAColor(30, OxyColors.Orange),
                    Text = "S3"
                };
                model.Annotations.Add(s3AreaTop);

                var s3AreaBottom = new RectangleAnnotation
                {
                    MinimumY = -MaxTorque, MaximumY = -RatedTorque,
                    Fill = OxyColor.FromAColor(30, OxyColors.Orange)
                };
                model.Annotations.Add(s3AreaBottom);
            }

            var scatterSeries = new ScatterSeries 
            { 
                MarkerType = MarkerType.Circle, 
                MarkerSize = 2, 
                MarkerFill = OxyColors.Blue,
                Title = "Operation Points"
            };

            double jTotal = _config.LoadInertia + _config.MotorInertia;

            foreach (var p in _lastResponse.Points)
            {
                // Use signed torque for 4-quadrant plot
                double torque = (jTotal * p.A) + (_config.FrictionTorque * Math.Sign(p.V));
                scatterSeries.Points.Add(new ScatterPoint(p.V, torque));
            }

            model.Series.Add(scatterSeries);
            return model;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
