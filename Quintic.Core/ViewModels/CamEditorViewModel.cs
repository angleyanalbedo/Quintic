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
    public class CamEditorViewModel : INotifyPropertyChanged
    {
        public class ProjectStateDto
        {
            public ProjectConfig Config { get; set; }
            public List<Segment> Segments { get; set; }
            public List<CamTrack> LogicTracks { get; set; }
            public double LimitVelocity { get; set; }
            public double LimitAcceleration { get; set; }
        }

        public ToolbarViewModel ToolbarVM { get; set; }
        public SegmentTableViewModel SegmentTableVM { get; set; }
        public LogicTracksViewModel LogicTracksVM { get; set; }
        public CamPlotViewModel CamPlotVM { get; set; }
        public KinematicAnalysisViewModel AnalysisVM { get; set; }
        
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
        private bool _isRecalculating = false;
        private bool _isDragging = false;

        public ICommand SaveProjectCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand ExportCsvCommand { get; private set; }
        public ICommand ExportStCommand { get; private set; }
        public ICommand ShowAnalysisCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        public Action RequestOpenAnalysisWindow { get; set; }

        private CalculationResponse _lastCalculation;

        public CamEditorViewModel()
        {
            Config = new ProjectConfig { Resolution = 1000 };
            
            SegmentTableVM = new SegmentTableViewModel();
            LogicTracksVM = new LogicTracksViewModel();
            CamPlotVM = new CamPlotViewModel();
            AnalysisVM = new KinematicAnalysisViewModel();

            // Subscribe to changes in TableVM to trigger Recalculate
            SegmentTableVM.SegmentsChanged += (s, e) => Recalculate();
            
            // Handle Dragging
            CamPlotVM.PointDragged += OnCamPointDragged;
            CamPlotVM.CanvasCtrlClicked += OnCanvasCtrlClicked;
            CamPlotVM.DragStarted += () => _isDragging = true;
            CamPlotVM.DragFinished += () => 
            { 
                _isDragging = false; 
                RecordSnapshot(); // Record final state after drag
            };

            SaveProjectCommand = new RelayCommand(ExecuteSaveProject);
            OpenProjectCommand = new RelayCommand(ExecuteOpenProject);
            ExportCsvCommand = new RelayCommand(ExecuteExportCsv);
            ExportStCommand = new RelayCommand(ExecuteExportSt);
            ShowAnalysisCommand = new RelayCommand(o => RequestOpenAnalysisWindow?.Invoke());
            UndoCommand = new RelayCommand(ExecuteUndo, o => _historyIndex > 0);
            RedoCommand = new RelayCommand(ExecuteRedo, o => _historyIndex < _history.Count - 1);
            
            ToolbarVM = new ToolbarViewModel(SaveProjectCommand, OpenProjectCommand, ExportCsvCommand, ExportStCommand, ShowAnalysisCommand, UndoCommand, RedoCommand);

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
                    LogicTracks = this.LogicTracksVM.Tracks.ToList(),
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
                    // Suppress snapshots during bulk update
                    _isNavigatingHistory = true;

                    var jsonString = File.ReadAllText(openFileDialog.FileName);
                    ApplyState(jsonString);
                    
                    _isNavigatingHistory = false;

                    // Record single snapshot for the loaded state
                    RecordSnapshot();
                }
                catch (Exception)
                {
                    _isNavigatingHistory = false;
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
                CommandManager.InvalidateRequerySuggested();
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
                CommandManager.InvalidateRequerySuggested();
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

                this.LogicTracksVM.Tracks.Clear();
                if (projectState.LogicTracks != null)
                {
                    foreach (var track in projectState.LogicTracks)
                    {
                        this.LogicTracksVM.Tracks.Add(track);
                    }
                }
            }
        }

        private void RecordSnapshot()
        {
            if (_isNavigatingHistory || _isDragging) return;

            var state = new ProjectStateDto
            {
                Config = this.Config,
                Segments = this.SegmentTableVM.Segments.ToList(),
                LogicTracks = this.LogicTracksVM.Tracks.ToList(),
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
            CommandManager.InvalidateRequerySuggested();
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

        private void ExecuteExportSt(object obj)
        {
            if (_lastCalculation == null || _lastCalculation.Points.Count == 0) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Structured Text (*.st)|*.st|Text File (*.txt)|*.txt",
                DefaultExt = ".st",
                FileName = "CamProfile.st"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                StCodeGenerator.Export(saveFileDialog.FileName, _lastCalculation);
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

        private void OnCanvasCtrlClicked(double x, double y)
        {
            // Find the segment containing x
            var targetSegment = SegmentTableVM.Segments.FirstOrDefault(s => 
                s.ComputedMasterStart <= x && s.ComputedMasterEnd > x);
            
            if (targetSegment == null) return;

            RecordSnapshot();

            // Calculate exact S at x to maintain continuity (Snap to curve)
            double splitS = y; 
            if (_lastCalculation != null)
            {
                // Find closest point in calculation result
                var p = _lastCalculation.Points.OrderBy(pt => Math.Abs(pt.Theta - x)).FirstOrDefault();
                if (p != null) splitS = p.S;
            }

            int index = SegmentTableVM.Segments.IndexOf(targetSegment);
            
            // Create new segment (Right part)
            var newSegment = new Segment 
            { 
                MotionLaw = targetSegment.MotionLaw,
                CoordinateMode = targetSegment.CoordinateMode,
                ReferenceType = targetSegment.ReferenceType
            };

            // Save original values
            double origMasterEnd = targetSegment.ComputedMasterEnd ?? 0;
            double origSlaveEnd = targetSegment.ComputedSlaveEnd ?? 0;
            double origMasterStart = targetSegment.ComputedMasterStart ?? 0;
            double origSlaveStart = targetSegment.ComputedSlaveStart ?? 0;

            // Update Left Segment (targetSegment)
            if (targetSegment.CoordinateMode == CoordinateMode.Absolute)
            {
                targetSegment.MasterVal = x;
                targetSegment.SlaveVal = splitS;
                
                newSegment.MasterVal = origMasterEnd;
                newSegment.SlaveVal = origSlaveEnd;
            }
            else // Relative
            {
                double splitMasterDelta = x - origMasterStart;
                double splitSlaveDelta = splitS - origSlaveStart;

                // Ensure we don't have negative or zero duration
                if (splitMasterDelta <= 0.001) splitMasterDelta = 0.001;

                targetSegment.MasterVal = splitMasterDelta;
                targetSegment.SlaveVal = splitSlaveDelta;

                newSegment.MasterVal = origMasterEnd - x; 
                newSegment.SlaveVal = origSlaveEnd - splitS;
            }

            // Insert new segment
            SegmentTableVM.Segments.Insert(index + 1, newSegment);
        }

        private void Recalculate()
        {
            if (_isRecalculating) return;
            _isRecalculating = true;

            try
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

                // 5. Update Analysis
                AnalysisVM.Update(_lastCalculation, Config);
            }
            finally
            {
                _isRecalculating = false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
