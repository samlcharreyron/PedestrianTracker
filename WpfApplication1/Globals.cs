using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PedestrianTracker.Properties;
using System.IO;

namespace PedestrianTracker
{
    public static class Globals
    {
        public static TrajectoryDbDataSet ds;
        public static string logpath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "debuglog.txt");
        
        public static void Log(string logMessage)
        {
            if (Settings.Default.Logging)
            {
                using (StreamWriter w = File.AppendText(logpath))
                {
                    w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w.WriteLine("  :");
                    w.WriteLine("  :{0}", logMessage);
                    w.WriteLine("-------------------------------");
                    // Update the underlying file.
                    w.Flush();
                }
            }
        }
    }
}
