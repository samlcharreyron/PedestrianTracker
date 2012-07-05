using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PedestrianTracker.RecoveryHelper
{

    public enum FileType
    {
        Xml,
        Binary
    }

    //copied from Code Pack to save calling code from taking a Code Pack dependency
    [Flags]
    public enum RestartRestrictions
    {
        // Summary:
        //     Always restart the application.
        None = 0,
        //
        // Summary:
        //     Do not restart when the application has crashed.
        NotOnCrash = 1,
        //
        // Summary:
        //     Do not restart when the application is hung.
        NotOnHang = 2,
        //
        // Summary:
        //     Do not restart when the application is terminated due to a system update.
        NotOnPatch = 4,
        //
        // Summary:
        //     Do not restart when the application is terminated because of a system reboot.
        NotOnReboot = 8,
    }

    class RestoreState<T> where T : new()
    {
        public string RecoveryPath;
        public uint PingInterval;
        public T Subject;
    }

    public class RestartRecoveryHelper<T> where T : new()
    {
        string RecoveryPath;
        static RestartRecoveryHelper()
        {
            //can't constrain T to be serializable (implementing ISerializable is too restrictive)
            //so checking in a static constructor as suggested in http://msdn.microsoft.com/en-us/library/aa479858.aspx
            if (!typeof(T).IsSerializable)
                throw new InvalidOperationException("Type is not serializable.");
        }

        public T CheckForRestart()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                //LogToFile("no platform support, returning nothing \r\n");
                return default(T);
            }

            //look at hint
            if (System.Environment.GetCommandLineArgs().Length <= 1)
            {
                //LogToFile("no hint, returning nothing \r\n");
                return default(T);
            }

            //parse out file name and type
            RecoveryPath = System.Environment.GetCommandLineArgs()[1];
            //LogToFile("===================== restart check\r\n");
            //LogToFile("Recovery Path: " + RecoveryPath + "\r\n");
            if (File.Exists(RecoveryPath))
            {

                T subject = new T();
                if (RecoveryPath.EndsWith(".xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    FileStream fs = new FileStream(RecoveryPath, FileMode.Open);
                    subject = (T)serializer.Deserialize(fs);
                    fs.Close();
                    //LogToFile("found xml. \r\n" + subject.ToString() + "\r\n");
                    //File.Delete(RecoveryPath);
        
                    return subject;
                }
                if (RecoveryPath.EndsWith(".bin"))
                {
                    IFormatter formatter = new BinaryFormatter();
                    StreamReader reader = File.OpenText(RecoveryPath);
                    subject = (T)formatter.Deserialize(reader.BaseStream);
                    reader.Close();
                    //LogToFile("found bin. \r\n" + subject.ToString() + "\r\n");
                    File.Delete(RecoveryPath);
                    return subject;
                }
                else
                {
                    // super unlikely. command line arg is a file, and it exists, but it's not 
                    // something we saved on a crash? Just shrug and don't return them anything.
                    //LogToFile("found strangeness, returning nothing \r\n");
                    return default(T);
                }
            }
            else
            {
                //either there was a command line arg that was not a restart hint, or it was
                //but the file has gone away somehow. do we communicate this to calling code?
                //current decision is no, just don't return them anything

                //LogToFile("can't find file, returning nothing \r\n");
                return default(T);
            }
        }

        private const int defaultPingInterval = 300; //milliseconds
        private const RestartRestrictions defaultRestartRestrictions = RestartRestrictions.None;
        private const string defaultInstanceName = "Recover";

        public void RegisterForRestartAndRecovery(string appName, string instanceName, T subject,
            FileType t)
        {
            RegisterForRestartAndRecovery(appName, instanceName, subject, defaultPingInterval, t, defaultRestartRestrictions);
        }

        public void RegisterForRestartAndRecovery(string appName, T subject, FileType t)
        {
            // call main version with "Recover" in file name, 300 ms ping interval, no restriction
            RegisterForRestartAndRecovery(appName, defaultInstanceName, subject, defaultPingInterval, t, defaultRestartRestrictions);
        }

        public void RegisterForRestartAndRecovery(string appName, string instanceName, T subject,
            uint pingInterval, FileType t, RestartRestrictions res)
        {
            //LogToFile("===================== registering \r\n");
            if (Environment.OSVersion.Version.Major < 6)
            {
                //LogToFile("no platform support, returning \r\n");
                return;
            }

            RecoveryPath = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), appName);
            if (!Directory.Exists(RecoveryPath))
            {
                //LogToFile("creating folder" + RecoveryPath + "\r\n");
                Directory.CreateDirectory(RecoveryPath);
            }
            string filename = instanceName + DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss-fff");
            if (t == FileType.Xml)
                filename = filename + ".xml";
            else
                filename = filename + ".bin";
            RecoveryPath = Path.Combine(RecoveryPath, filename);
            Microsoft.WindowsAPICodePack.ApplicationServices.RestartRestrictions restrictions
                = (Microsoft.WindowsAPICodePack.ApplicationServices.RestartRestrictions)res;

            ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings(RecoveryPath,
                                                restrictions));

            RestoreState<T> rs = new RestoreState<T>();
            rs.RecoveryPath = RecoveryPath;
            rs.PingInterval = pingInterval;
            rs.Subject = subject;
            RecoveryData data = new RecoveryData(new RecoveryCallback(RecoveryProcedure), rs);
            RecoverySettings settings = new RecoverySettings(data, pingInterval);

            ApplicationRestartRecoveryManager.RegisterForApplicationRecovery(settings);

        }
        private int RecoveryProcedure(object state)
        {
            //async polling see http://msdn.microsoft.com/en-us/library/2e08f6yc(VS.71).aspx

            //LogToFile("===================== recovery called \r\n");
            RestoreState<T> rs = (RestoreState<T>)state;
            T document = rs.Subject;
            uint pingInterval = rs.PingInterval;
            int sleepInterval = (int)pingInterval / 3;
            RecoveryPath = rs.RecoveryPath;

            bool userCanceledRecovery = ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();

            AsyncDelegate d = new AsyncDelegate(SerializeSubject);
            IAsyncResult ar = d.BeginInvoke(document, null, null);

            while (!userCanceledRecovery && ar.IsCompleted == false)
            {
                System.Threading.Thread.Sleep(sleepInterval);
                userCanceledRecovery = ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();
            }
            if (userCanceledRecovery)
                Environment.FailFast("user canceled recovery");

            d.EndInvoke(ar);

            ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(true);
            return 0;
        }

        private delegate void AsyncDelegate(T document);

        private void SerializeSubject(T document)
        {
            //sleep for ten seconds to simulate slow save
            //System.Threading.Thread.Sleep(10000);

            //LogToFile("writing to " + RecoveryPath + "\r\n");
            if (RecoveryPath.EndsWith(".xml"))
            {
                //using (Stream createFile = File.OpenWrite(RecoveryPath))
                //{
                //    XmlSerializer ser = new XmlSerializer(document.GetType());
                //    ser.Serialize(createFile, document);
                //    //LogToFile("wrote xml\r\n");
                //}

                using (Stream createFile = File.OpenWrite(RecoveryPath))
                {
                    Globals.ds.WriteXml(createFile,System.Data.XmlWriteMode.WriteSchema);
                }
            }
            else
            {
                using (Stream createFile = File.Open(RecoveryPath, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(createFile, document);
                    //LogToFile("wrote bin\r\n");
                }
            }
        }
        private void LogToFile(string s)
        {
            string logpath = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "DebugAppRecov.txt");

            File.AppendAllText(logpath, s);
        }
    }
}

