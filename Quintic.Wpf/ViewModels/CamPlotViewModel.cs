using OxyPlot;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Services;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class CamPlotViewModel : INotifyPropertyChanged
    {
        private readonly PlotService _plotService;

        public PlotModel SPlotModel => _plotService.SPlotModel;
        public PlotModel VAPlotModel => _plotService.VAPlotModel;

        public event System.Action<int, double, double> PointDragged;

        public ICommand ResetViewCommand { get; private set; }

        private bool _isVelocityVisible = true;
        public bool IsVelocityVisible
        {
            get => _isVelocityVisible;
            set
            {
                if (_isVelocityVisible != value)
                {
                    _isVelocityVisible = value;
                    OnPropertyChanged();
                    _plotService.SetSeriesVisibility("V", value);
                }
            }
        }

        private bool _isAccelerationVisible = true;
        public bool IsAccelerationVisible
        {
            get => _isAccelerationVisible;
            set
            {
                if (_isAccelerationVisible != value)
                {
                    _isAccelerationVisible = value;
                    OnPropertyChanged();
                    _plotService.SetSeriesVisibility("A", value);
                }
            }
        }

        private bool _isJerkVisible = true;
        public bool IsJerkVisible
        {
            get => _isJerkVisible;
            set
            {
                if (_isJerkVisible != value)
                {
                    _isJerkVisible = value;
                    OnPropertyChanged();
                    _plotService.SetSeriesVisibility("J", value);
                }
            }
        }

        public CamPlotViewModel()
        {
            _plotService = new PlotService();
            _plotService.PointDragged += (i, m, s) => PointDragged?.Invoke(i, m, s);
            ResetViewCommand = new RelayCommand(o => _plotService.ResetAxes());
        }

        public void UpdatePlots(CalculationResponse response, IEnumerable<Segment> segments = null)
        {
            _plotService.UpdatePlots(response, segments);
        }

        public void UpdateLimits(double maxV, double maxA)
        {
            _plotService.UpdateLimitLines(maxV, maxA);
        }

        public void HighlightViolations(CalculationResponse response, double maxV, double maxA)
        {
            _plotService.HighlightLimitViolations(response, maxV, maxA);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
