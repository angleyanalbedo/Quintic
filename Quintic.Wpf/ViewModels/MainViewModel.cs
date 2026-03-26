using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.Win32;

using Quintic.Wpf.Core.Models;
using Quintic.Wpf.Core.Services;
using Quintic.Wpf.Core.Commands;

namespace Quintic.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public class ProjectStateDto
        {
            public ProjectConfig Config { get; set; }
            public List<Segment> Segments { get; set; }
            public double LimitVelocity { get; set; }
            public double LimitAcceleration { get; set; }
        }

        public ToolbarViewModel ToolbarVM { get; set; }
        public SegmentTableViewModel SegmentTableVM { get; set; }
        public CamPlotViewModel CamPlotVM { get; set; }
        
        public ProjectConfig Config { get; set; }
        
        // Limits
        private double _limitVelocity = 1000;
        public double LimitVelocity
        {
            get => _limitVelocity;
            set { _limitVelocity = value; OnPropertyChanged(); Recalculate(); }
        }

        private double _limitAcceleration = 10000;
        public double LimitAcceleration
        {
            get => _limitAcceleration;
            set { _limitAcceleration = value; OnPropertyChanged(); Recalculate(); }
        }

        // History
        private readonly List<string> _history = new List<string>();
        private int _historyIndex = -1;
        private bool _isNavigatingHistory = false;

        public ICommand SaveProjectCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand ExportCsvCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        private CalculationResponse _lastCalculation;

        public MainViewModel()
        {
            Config = new ProjectConfig { Resolution = 1000 };
            
            SegmentTableVM = new SegmentTableViewModel();
            CamPlotVM = new CamPlotViewModel();

            // Subscribe to changes in TableVM to trigger Recalculate
            SegmentTableVM.SegmentsChanged += (s, e) => Recalculate();
            
            // Handle Dragging
            CamPlotVM.PointDragged += OnCamPointDragged;

            SaveProjectCommand = new RelayCommand(ExecuteSaveProject);
            OpenProjectCommand = new RelayCommand(ExecuteOpenProject);
            ExportCsvCommand = new RelayCommand(ExecuteExportCsv);
            UndoCommand = new RelayCommand(ExecuteUndo, o => _historyIndex > 0);
            RedoCommand = new RelayCommand(ExecuteRedo, o => _historyIndex < _history.Count - 1);
            
            ToolbarVM = new ToolbarViewModel(SaveProjectCommand, OpenProjectCommand, ExportCsvCommand, UndoCommand, RedoCommand);

            Recalculate();
        }

        private void ExecuteSaveProject(object obj)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Quintic Project (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "Project.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var projectState = new ProjectStateDto
                {
                    Config = this.Config,
                    Segments = this.SegmentTableVM.Segments.ToList(),
                    LimitVelocity = this.LimitVelocity,
                    LimitAcceleration = this.LimitAcceleration
                };

                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true, 
                    ReferenceHandler = ReferenceHandler.IgnoreCycles 
                };
                var jsonString = JsonSerializer.Serialize(projectState, options);
                File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
        }

        private void ExecuteOpenProject(object obj)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Quintic Project (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var jsonString = File.ReadAllText(openFileDialog.FileName);
                    ApplyState(jsonString);
                    // Clear history after open? Or treat open as a new state?
                    // Let's treat it as a new state in history
                }
                catch (Exception)
                {
                    // Handle error silently or log
                }
            }
        }

        private void ExecuteUndo(object obj)
        {
            if (_historyIndex > 0)
            {
                _isNavigatingHistory = true;
                _historyIndex--;
                ApplyState(_history[_historyIndex]);
                _isNavigatingHistory = false;
            }
        }

        private void ExecuteRedo(object obj)
        {
            if (_historyIndex < _history.Count - 1)
            {
                _isNavigatingHistory = true;
                _historyIndex++;
                ApplyState(_history[_historyIndex]);
                _isNavigatingHistory = false;
            }
        }

        private void ApplyState(string jsonString)
        {
            var projectState = JsonSerializer.Deserialize<ProjectStateDto>(jsonString);

            if (projectState != null)
            {
                this.Config = projectState.Config;
                this.LimitVelocity = projectState.LimitVelocity;
                this.LimitAcceleration = projectState.LimitAcceleration;

                this.SegmentTableVM.Segments.Clear();
                foreach (var seg in projectState.Segments)
                {
                    this.SegmentTableVM.Segments.Add(seg);
                }
            }
        }

        private void RecordSnapshot()
        {
            if (_isNavigatingHistory) return;

            var state = new ProjectStateDto
            {
                Config = this.Config,
                Segments = this.SegmentTableVM.Segments.ToList(),
                LimitVelocity = this.LimitVelocity,
                LimitAcceleration = this.LimitAcceleration
            };
            
            var options = new JsonSerializerOptions 
            { 
                ReferenceHandler = ReferenceHandler.IgnoreCycles 
            };
            var json = JsonSerializer.Serialize(state, options);

            // Avoid duplicates
            if (_historyIndex >= 0 && _historyIndex < _history.Count && _history[_historyIndex] == json) return;

            // Remove future history if we branched off
            if (_historyIndex < _history.Count - 1)
            {
                _history.RemoveRange(_historyIndex + 1, _history.Count - (_historyIndex + 1));
            }

            _history.Add(json);
            _historyIndex++;
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

        private void OnCamPointDragged(int index, double newM, double newS)
        {
            if (index < 0 || index >= SegmentTableVM.Segments.Count) return;

            var seg = SegmentTableVM.Segments[index];
            var prevM = seg.ComputedMasterStart ?? 0;
            var prevS = seg.ComputedSlaveStart ?? 0;

            // Constraint: Master must be > Start
            if (newM <= prevM) newM = prevM + 0.1;

            // Update Model based on Coordinate Mode
            if (seg.CoordinateMode == CoordinateMode.Absolute)
            {
                seg.MasterVal = newM;
                seg.SlaveVal = newS;
            }
            else
            {
                seg.MasterVal = newM - prevM;
                seg.SlaveVal = newS - prevS;
            }
            // PropertyChanged will trigger Recalculate via SegmentTableVM listener
        }

        private void Recalculate()
        {
            RecordSnapshot();

            // 1. Update Computed Values in Segments (Business Logic)
            double currentM = 0;
            double currentS = 0;
            foreach (var seg in SegmentTableVM.Segments)
            {
                seg.IsLimitExceeded = false; // Reset flag
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

            // Validation: Check for monotonic master position
            foreach (var seg in SegmentTableVM.Segments)
            {
                if (seg.ComputedMasterEnd <= seg.ComputedMasterStart)
                {
                    return;
                }
            }

            // 2. Calculate Profile
            _lastCalculation = CamCalculator.CalculateProject(SegmentTableVM.Segments.ToList(), Config);
            
            // 3. Check Limits
            if (_lastCalculation != null)
            {
                foreach (var p in _lastCalculation.Points)
                {
                    if (Math.Abs(p.V) > LimitVelocity || Math.Abs(p.A) > LimitAcceleration)
                    {
                        // Find which segment this point belongs to
                        var seg = SegmentTableVM.Segments.FirstOrDefault(s => 
                            p.Theta >= s.ComputedMasterStart && p.Theta <= s.ComputedMasterEnd);
                        
                        if (seg != null) seg.IsLimitExceeded = true;
                    }
                }
            }

            // 4. Update Plots
            CamPlotVM.UpdatePlots(_lastCalculation, SegmentTableVM.Segments);
            CamPlotVM.UpdateLimits(LimitVelocity, LimitAcceleration);
            CamPlotVM.HighlightViolations(_lastCalculation, LimitVelocity, LimitAcceleration);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
