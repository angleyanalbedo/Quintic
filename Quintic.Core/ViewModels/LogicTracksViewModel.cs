using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class LogicTracksViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CamTrack> Tracks { get; set; }

        public ICommand AddTrackCommand { get; private set; }
        public ICommand RemoveTrackCommand { get; private set; }

        public LogicTracksViewModel()
        {
            Tracks = new ObservableCollection<CamTrack>();
            AddTrackCommand = new RelayCommand(ExecuteAddTrack);
            RemoveTrackCommand = new RelayCommand(ExecuteRemoveTrack);
        }

        private void ExecuteAddTrack(object obj)
        {
            Tracks.Add(new CamTrack { Name = $"Track {Tracks.Count}", ChannelIndex = Tracks.Count });
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
