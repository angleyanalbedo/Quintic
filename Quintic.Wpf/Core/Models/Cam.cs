using System.Collections.Generic;

namespace Quintic.Wpf.Core.Models
{
    public enum MotionLawType
    {
        Polynomial5,
        ModifiedSine,
        Cycloidal,
        ConstantVelocity,
        ModifiedTrapezoid,
        Trapezoidal,
        Dwell
    }

    public enum CoordinateMode
    {
        Absolute,
        Relative
    }

    public enum ReferenceType
    {
        Master,
        Time
    }

    public enum ExecutionMode
    {
        OneShot,
        Cyclic
    }

    public class ProjectConfig
    {
        public double MasterVelocity { get; set; } = 60.0; // RPM
        public int Resolution { get; set; } = 360;
        public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.OneShot;
        public string UnitsMaster { get; set; } = "deg";
        public string UnitsSlave { get; set; } = "mm";
    }

    public class CamPoint
    {
        public double Theta { get; set; }
        public double S { get; set; }
        public double V { get; set; }
        public double A { get; set; }
        public double J { get; set; }

        public CamPoint(double theta, double s, double v, double a, double j)
        {
            Theta = theta;
            S = s;
            V = v;
            A = a;
            J = j;
        }
    }

    public class CalculationResponse
    {
        public List<CamPoint> Points { get; set; } = new List<CamPoint>();
        public double MaxVelocity { get; set; }
        public double MaxAcceleration { get; set; }
        public double MaxJerk { get; set; }
    }
}
