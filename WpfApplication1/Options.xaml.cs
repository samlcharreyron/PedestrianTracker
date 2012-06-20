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
using PedestrianTracker.Properties;

namespace PedestrianTracker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class Options : Window
    {

        public Options()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        public static bool TrySetAngle(int angle)
        {
            try
            {
                MainWindow.myKinect.ElevationAngle = angle;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void ElevationAngleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TrySetAngle((int)e.NewValue);
        }
 
    }
}
