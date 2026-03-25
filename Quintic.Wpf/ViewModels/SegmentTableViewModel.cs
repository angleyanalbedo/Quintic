using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Quintic.Wpf.Core.Models;

namespace Quintic.Wpf.ViewModels
{
    public class SegmentTableViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Segment> Segments { get; set; }

        // Event to notify parent when recalculation is needed
        public event EventHandler SegmentsChanged;

        public SegmentTableViewModel()
        {
            Segments = new ObservableCollection<Segment>
            {
                new Segment { MasterVal = 90, SlaveVal = 50, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 180, SlaveVal = 50, MotionLaw = MotionLawType.Dwell, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 360, SlaveVal = 0, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute }
            };

            Segments.CollectionChanged += OnSegmentsCollectionChanged;
            foreach (var seg in Segments)
            {
                seg.PropertyChanged += OnSegmentPropertyChanged;
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
            SegmentsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSegmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("Computed")) return;
            SegmentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
