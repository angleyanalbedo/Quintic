using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Specialized;
using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class LogicTracksViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CamTrack> Tracks { get; set; }

        private CamTrack _selectedTrack;
        public CamTrack SelectedTrack
        {
            get => _selectedTrack;
            set 
            { 
                _selectedTrack = value; 
                OnPropertyChanged(); 
                // Re-evaluate command can execute if needed
            }
        }

        public ICommand AddTrackCommand { get; private set; }
        public ICommand RemoveTrackCommand { get; private set; }
        public ICommand AddSwitchCommand { get; private set; }
        public ICommand RemoveSwitchCommand { get; private set; }

        public event EventHandler TracksChanged;

        public LogicTracksViewModel()
        {
            Tracks = new ObservableCollection<CamTrack>();
            Tracks.CollectionChanged += OnTracksCollectionChanged;

            AddTrackCommand = new RelayCommand(ExecuteAddTrack);
            RemoveTrackCommand = new RelayCommand(ExecuteRemoveTrack);
            AddSwitchCommand = new RelayCommand(ExecuteAddSwitch, CanExecuteSwitchCommand);
            RemoveSwitchCommand = new RelayCommand(ExecuteRemoveSwitch);
        }

        private void ExecuteAddSwitch(object obj)
        {
            if (SelectedTrack != null)
            {
                SelectedTrack.Switches.Add(new CamSwitch { OnAngle = 0, OffAngle = 180 });
            }
        }

        private void ExecuteRemoveSwitch(object obj)
        {
            if (SelectedTrack != null && obj is CamSwitch sw)
            {
                SelectedTrack.Switches.Remove(sw);
            }
        }

        private bool CanExecuteSwitchCommand(object obj)
        {
            return SelectedTrack != null;
        }

        private void OnTracksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CamTrack track in e.NewItems)
                {
                    track.PropertyChanged += OnTrackPropertyChanged;
                    if (track.Switches != null)
                    {
                        track.Switches.CollectionChanged += OnSwitchesCollectionChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (CamTrack track in e.OldItems)
                {
                    track.PropertyChanged -= OnTrackPropertyChanged;
                    if (track.Switches != null)
                    {
                        track.Switches.CollectionChanged -= OnSwitchesCollectionChanged;
                    }
                }
            }

            TracksChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTrackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TracksChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSwitchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CamSwitch sw in e.NewItems)
                    sw.PropertyChanged += OnSwitchPropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (CamSwitch sw in e.OldItems)
                    sw.PropertyChanged -= OnSwitchPropertyChanged;
            }
            TracksChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSwitchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TracksChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteAddTrack(object obj)
        {
            var newTrack = new CamTrack { Name = $"Track {Tracks.Count}", ChannelIndex = Tracks.Count };
            // Give it a default switch so the user can see something immediately
            newTrack.Switches.Add(new CamSwitch { OnAngle = 30, OffAngle = 90 });
            Tracks.Add(newTrack);
        }

        private void ExecuteRemoveTrack(object obj)
        {
            if (obj is CamTrack track)
            {
                Tracks.Remove(track);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
