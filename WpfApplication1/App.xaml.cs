using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PedestrianTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //AppDomain.CurrentDomain.SetData("DataDirectory", Environment.SpecialFolder.ApplicationData);
            //MessageBox.Show(Environment.SpecialFolder.ApplicationData as string);
        }
    }
}
