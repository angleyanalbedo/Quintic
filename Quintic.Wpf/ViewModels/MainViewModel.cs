using System;
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
        public SegmentTableViewModel SegmentTableVM { get; set; }
        public CamPlotViewModel CamPlotVM { get; set; }
        
        public ProjectConfig Config { get; set; }
        public ICommand ExportCsvCommand { get; private set; }

        private CalculationResponse _lastCalculation;

        public MainViewModel()
        {
            Config = new ProjectConfig { Resolution = 1000 };
            
            SegmentTableVM = new SegmentTableViewModel();
            CamPlotVM = new CamPlotViewModel();

            // Subscribe to changes in TableVM to trigger Recalculate
            SegmentTableVM.SegmentsChanged += (s, e) => Recalculate();

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

        private void Recalculate()
        {
            // 1. Update Computed Values in Segments (Business Logic)
            double currentM = 0;
            double currentS = 0;
            foreach (var seg in SegmentTableVM.Segments)
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

            // 2. Calculate Profile
            _lastCalculation = CamCalculator.CalculateProject(SegmentTableVM.Segments.ToList(), Config);
            
            // 3. Update Plots
            CamPlotVM.UpdatePlots(_lastCalculation);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
