using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Quintic.Wpf.ViewModels
{
    public class ToolbarViewModel : INotifyPropertyChanged
    {
        public ICommand SaveProjectCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand ExportCsvCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        public ToolbarViewModel(ICommand saveProjectCommand, ICommand openProjectCommand, ICommand exportCsvCommand, ICommand undoCommand, ICommand redoCommand)
        {
            SaveProjectCommand = saveProjectCommand;
            OpenProjectCommand = openProjectCommand;
            ExportCsvCommand = exportCsvCommand;
            UndoCommand = undoCommand;
            RedoCommand = redoCommand;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
