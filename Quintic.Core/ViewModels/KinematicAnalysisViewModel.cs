using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Quintic.Wpf.Core.Models;

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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
