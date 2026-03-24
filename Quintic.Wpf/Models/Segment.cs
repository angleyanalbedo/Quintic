namespace Quintic.Wpf.Models
{
    public class Segment
    {
        public double MasterStart { get; set; }
        public double MasterEnd { get; set; }
        public double SlaveStart { get; set; }
        public double SlaveEnd { get; set; }
        public string MotionLaw { get; set; } = "Quintic";
    }
}
