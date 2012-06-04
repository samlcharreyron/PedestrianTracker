using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace PedestrianTracker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TrajectoryWindow : Window
    {
        public TrajectoryWindow()
        {
            InitializeComponent();

            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            TrajectoryDbDataSet trajectoryDbDataSet = Globals.ds;
            // Load data into the table trajectories. You can modify this code as needed.
            //TrajectoryDbDataSetTableAdapters.trajectoriesTableAdapter trajectoryDbDataSettrajectoriesTableAdapter = new TrajectoryDbDataSetTableAdapters.trajectoriesTableAdapter();
            //trajectoryDbDataSettrajectoriesTableAdapter.ClearBeforeFill = false;
            //trajectoryDbDataSettrajectoriesTableAdapter.Fill(trajectoryDbDataSet.trajectories);
            this.trajectoriesDataGrid.ItemsSource = Globals.ds.trajectories;
            //CollectionViewSource trajectoriesViewSource = ((CollectionViewSource)(this.FindResource("trajectoriesViewSource")));
            //trajectoriesViewSource.View.MoveCurrentToFirst();
        }

        private void trajectoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
