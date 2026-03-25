using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Quintic.Wpf.Core.Models
{
    public class Segment : INotifyPropertyChanged
    {
        private string _id;
        private MotionLawType _motionLaw = MotionLawType.Polynomial5;
        private CoordinateMode _coordinateMode = CoordinateMode.Absolute;
        private ReferenceType _referenceType = ReferenceType.Master;
        private double _masterVal;
        private double _slaveVal;
        
        // Boundary Conditions (Optional)
        private double _startVelocity;
        private double _endVelocity;
        private double _startAcceleration;
        private double _endAcceleration;

        // Computed Fields (set by compiler)
        private double? _computedMasterStart;
        private double? _computedMasterEnd;
        private double? _computedSlaveStart;
        private double? _computedSlaveEnd;

        public Segment()
        {
            _id = Guid.NewGuid().ToString();
        }

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public MotionLawType MotionLaw
        {
            get => _motionLaw;
            set { _motionLaw = value; OnPropertyChanged(); }
        }

        public CoordinateMode CoordinateMode
        {
            get => _coordinateMode;
            set { _coordinateMode = value; OnPropertyChanged(); }
        }

        public ReferenceType ReferenceType
        {
            get => _referenceType;
            set { _referenceType = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Absolute Master Position OR Delta Master OR Duration (seconds)
        /// </summary>
        public double MasterVal
        {
            get => _masterVal;
            set { _masterVal = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Absolute Slave Position OR Delta Slave
        /// </summary>
        public double SlaveVal
        {
            get => _slaveVal;
            set { _slaveVal = value; OnPropertyChanged(); }
        }

        public double StartVelocity
        {
            get => _startVelocity;
            set { _startVelocity = value; OnPropertyChanged(); }
        }

        public double EndVelocity
        {
            get => _endVelocity;
            set { _endVelocity = value; OnPropertyChanged(); }
        }

        public double StartAcceleration
        {
            get => _startAcceleration;
            set { _startAcceleration = value; OnPropertyChanged(); }
        }

        public double EndAcceleration
        {
            get => _endAcceleration;
            set { _endAcceleration = value; OnPropertyChanged(); }
        }

        // --- Computed Properties (Read-Only Logic, but settable by Compiler) ---
        public double? ComputedMasterStart
        {
            get => _computedMasterStart;
            set { _computedMasterStart = value; OnPropertyChanged(); }
        }

        public double? ComputedMasterEnd
        {
            get => _computedMasterEnd;
            set { _computedMasterEnd = value; OnPropertyChanged(); }
        }

        public double? ComputedSlaveStart
        {
            get => _computedSlaveStart;
            set { _computedSlaveStart = value; OnPropertyChanged(); }
        }

        public double? ComputedSlaveEnd
        {
            get => _computedSlaveEnd;
            set { _computedSlaveEnd = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Helper method to clone the segment for the compiler
        /// </summary>
        public Segment Clone()
        {
            return (Segment)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
