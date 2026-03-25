using OxyPlot;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;

using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Services;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PlotService _plotService;

        public PlotModel SPlotModel => _plotService.SPlotModel;
        public PlotModel VAPlotModel => _plotService.VAPlotModel;
        public ObservableCollection<Segment> Segments { get; set; }
        public ProjectConfig Config { get; set; }
        public ICommand ExportCsvCommand { get; private set; }

        private CalculationResponse _lastCalculation;

        public MainViewModel()
        {
            _plotService = new PlotService();
            Config = new ProjectConfig { Resolution = 1000 };

            Segments = new ObservableCollection<Segment>
            {
                new Segment { MasterVal = 90, SlaveVal = 50, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 180, SlaveVal = 50, MotionLaw = MotionLawType.Dwell, CoordinateMode = CoordinateMode.Absolute },
                new Segment { MasterVal = 360, SlaveVal = 0, MotionLaw = MotionLawType.Polynomial5, CoordinateMode = CoordinateMode.Absolute }
            };

            // Event Subscriptions
            Segments.CollectionChanged += OnSegmentsCollectionChanged;
            foreach (var seg in Segments)
            {
                seg.PropertyChanged += OnSegmentPropertyChanged;
            }

            ExportCsvCommand = new RelayCommand(ExecuteExportCsv);

            Recalculate();
        }

        private void ExecuteExportCsv(object obj)
        {
            if (_lastCalculation == null || _lastCalculation.Points.Count == 0) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv|Text File (*.txt)|*.txt",
                DefaultExt = ".csv",
                FileName = "CamProfile.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                CsvExporter.Export(saveFileDialog.FileName, _lastCalculation);
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
            Recalculate();
        }

        private void OnSegmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("Computed")) return;
            Recalculate();
        }

        private void Recalculate()
        {
            _lastCalculation = CamCalculator.CalculateProject(Segments.ToList(), Config);
            _plotService.UpdatePlots(_lastCalculation);

            double currentM = 0;
            double currentS = 0;
            foreach (var seg in Segments)
            {
                seg.ComputedMasterStart = currentM;
                seg.ComputedSlaveStart = currentS;
                
                if (seg.CoordinateMode == CoordinateMode.Absolute)
                {
                    seg.ComputedMasterEnd = seg.MasterVal;
                    seg.ComputedSlaveEnd = seg.SlaveVal;
                }
                else
                {
                    seg.ComputedMasterEnd = currentM + seg.MasterVal;
                    seg.ComputedSlaveEnd = currentS + seg.SlaveVal;
                }
                
                currentM = seg.ComputedMasterEnd ?? 0;
                currentS = seg.ComputedSlaveEnd ?? 0;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
