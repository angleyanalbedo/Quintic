using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Quintic.Wpf.Core.Models;

namespace Quintic.Wpf.Views
{
    public partial class ControlPointEditorWindow : Window
    {
        public ObservableCollection<CamPoint> Points { get; set; }
        private List<CamPoint> _originalList;

        public ControlPointEditorWindow(List<CamPoint> points)
        {
            InitializeComponent();
            _originalList = points;
            
            // Clone points to allow cancellation
            Points = new ObservableCollection<CamPoint>(points.Select(p => new CamPoint(p.Theta, p.S, p.V, p.A, p.J)));
            
            PointsGrid.ItemsSource = Points;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _originalList.Clear();
            _originalList.AddRange(Points);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
