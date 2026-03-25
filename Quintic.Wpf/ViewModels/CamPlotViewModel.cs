using OxyPlot;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Services;

namespace Quintic.Wpf.ViewModels
{
    public class CamPlotViewModel : INotifyPropertyChanged
    {
        private readonly PlotService _plotService;

        public PlotModel SPlotModel => _plotService.SPlotModel;
        public PlotModel VAPlotModel => _plotService.VAPlotModel;

        public CamPlotViewModel()
        {
            _plotService = new PlotService();
        }

        public void UpdatePlots(CalculationResponse response)
        {
            _plotService.UpdatePlots(response);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
