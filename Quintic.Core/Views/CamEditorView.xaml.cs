using System.Windows.Controls;
using Quintic.Wpf.ViewModels;

namespace Quintic.Wpf.Views
{
    public partial class CamEditorView : UserControl
    {
        private KinematicAnalysisWindow _analysisWindow;

        public CamEditorView()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CamEditorViewModel vm)
            {
                vm.RequestOpenAnalysisWindow = () =>
                {
                    if (_analysisWindow == null || !_analysisWindow.IsLoaded)
                    {
                        _analysisWindow = new KinematicAnalysisWindow
                        {
                            DataContext = vm.AnalysisVM,
                            Owner = System.Windows.Window.GetWindow(this)
                        };
                        _analysisWindow.Closed += (s, args) => _analysisWindow = null;
                        _analysisWindow.Show();
                    }
                    else
                    {
                        _analysisWindow.Activate();
                    }
                };
            }
        }
    }
}
