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
using System.ComponentModel;

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
            //this.trajectoriesDataGrid.DataContext = Globals.ds.trajectories;
            
            BindingListCollectionView view = CollectionViewSource.GetDefaultView(Globals.ds.trajectories) as BindingListCollectionView;

            view.CustomFilter = "length > 0";

            this.trajectoriesDataGrid.ItemsSource = view;


        }

        private void trajectoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }       

    }
}
