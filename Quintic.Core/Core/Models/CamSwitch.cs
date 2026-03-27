using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Quintic.Wpf.Core.Models
{
    public class CamSwitch : INotifyPropertyChanged
    {
        private double _onAngle;
        private double _offAngle;
        private double _hysteresis;

        public double OnAngle
        {
            get => _onAngle;
            set { _onAngle = value; OnPropertyChanged(); }
        }

        public double OffAngle
        {
            get => _offAngle;
            set { _offAngle = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Hysteresis compensation in degrees.
        /// </summary>
        public double Hysteresis
        {
            get => _hysteresis;
            set { _hysteresis = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
