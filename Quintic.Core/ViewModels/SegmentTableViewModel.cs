using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class SegmentTableViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Segment> Segments { get; set; }

        private Segment _selectedSegment;
        public Segment SelectedSegment
        {
            get => _selectedSegment;
            set { _selectedSegment = value; OnPropertyChanged(); }
        }

        public ICommand AddSegmentCommand { get; private set; }
        public ICommand RemoveSegmentCommand { get; private set; }
        public ICommand EditControlPointsCommand { get; private set; }

        // Event to notify parent when recalculation is needed
        public event EventHandler SegmentsChanged;

        public SegmentTableViewModel()
        {
            AddSegmentCommand = new RelayCommand(ExecuteAddSegment);
            RemoveSegmentCommand = new RelayCommand(ExecuteRemoveSegment, CanExecuteRemoveSegment);
            EditControlPointsCommand = new RelayCommand(ExecuteEditControlPoints);

            Segments = new ObservableCollection<Segment>
            {
                new Segment { MasterVal = 90, SlaveVal = 50, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 180, SlaveVal = 50, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute },
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

        private void ExecuteAddSegment(object obj)
        {
            var newSeg = new Segment
            {
                MotionLaw = MotionLawType.Polynomial5,
                CoordinateMode = CoordinateMode.Relative,
                MasterVal = 90,
                SlaveVal = 0
            };

            if (SelectedSegment != null)
            {
                int index = Segments.IndexOf(SelectedSegment);
                Segments.Insert(index + 1, newSeg);
            }
            else
            {
                Segments.Add(newSeg);
            }
        }

        private void ExecuteRemoveSegment(object obj)
        {
            if (SelectedSegment != null)
            {
                Segments.Remove(SelectedSegment);
            }
        }

        private bool CanExecuteRemoveSegment(object obj)
        {
            return SelectedSegment != null && Segments.Count > 1;
        }

        private void ExecuteEditControlPoints(object obj)
        {
            if (obj is Segment segment)
            {
                var editor = new Quintic.Wpf.Views.ControlPointEditorWindow(segment.ControlPoints);
                if (editor.ShowDialog() == true)
                {
                    // Force property change notification to trigger recalculation
                    // Since we modified the list content in-place, the Segment.ControlPoints property setter wasn't called.
                    // We re-assign the list to itself (or a copy) to trigger OnPropertyChanged("ControlPoints")
                    var updatedList = new System.Collections.Generic.List<CamPoint>(segment.ControlPoints);
                    segment.ControlPoints = updatedList;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
