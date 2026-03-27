using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Quintic.Wpf.Core.Models
{
    public class CamTrack : INotifyPropertyChanged
    {
        private int _channelIndex;
        private string _name;
        private ObservableCollection<CamSwitch> _switches;

        public CamTrack()
        {
            _switches = new ObservableCollection<CamSwitch>();
        }

        public int ChannelIndex
        {
            get => _channelIndex;
            set { _channelIndex = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CamSwitch> Switches
        {
            get => _switches;
            set { _switches = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
