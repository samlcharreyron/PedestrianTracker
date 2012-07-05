using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Timers;
using Petzold.Media3D;
using Microsoft.Kinect;
using PedestrianTracker.Properties;
using Microsoft.WindowsAPICodePack.ApplicationServices;

//For testing
using System.Diagnostics;

namespace PedestrianTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static KinectSensor myKinect;
        private string errorMessage;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        private const float RenderWidth = 640f;
        private const float RenderHeight = 480f;

        private const int numberOfSkeletons = 6;

        private const double DistanceThreshold = 0.1;

        //Windows
        TrajectoryWindow tw1;
        Options ops;

        //Floor clipping plane
        Double A, B, C, D;
        private const int planeDepth = 60;
        private const int planeWidth = 60;

        //Drawing
        private readonly Brush centerPointBrush = Brushes.Black;
        private const double BodyCenterThickness = 8;

        private WireLines line;
        private ModelVisual3D planeModel;
        private Axes axes;

        Skeleton[] skeletons = new Skeleton[numberOfSkeletons];

        private List<Trajectory> trajectories;

        private Trajectory trajectoryCanvas;

        private DrawingGroup dv;

        //Database stuff
        private System.Data.SqlClient.SqlConnection connection;
        public const string connectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\TrajectoryDb.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";
        private Timer dbTimer;
        private Timer xmlTimer;
        private const double dbTimerSetting = 300000;
        private string xmlfilename = "";

        //Dataset stuff
        private DataRow lastDataRow;


        //For framerate
        protected int TotalFrames{ get; set; }
        protected int LastFrames { get; set; }
        private DateTime lastTime = DateTime.MaxValue;
        private int frameRate;

        public int FrameRate
        {
            get
            {
                return this.frameRate;
            }
            set
            {
                if (this.frameRate != value)
                {
                    this.frameRate = value;
                    this.NotifyPropertyChanged("FrameRate");
                }
            }
        }

        //Dependency Properties
        public static readonly DependencyProperty TotalPlayersProperty =
                DependencyProperty.Register("TotalPlayers", typeof(int), typeof(MainWindow), new UIPropertyMetadata(0));

        public static readonly DependencyProperty PedestrianCountsProperty =
                DependencyProperty.Register("PedestrianCounts", typeof(int), typeof(MainWindow), new UIPropertyMetadata(0));

        public static readonly DependencyProperty SkeletonXProperty =
                DependencyProperty.Register("SkeletonX", typeof(int), typeof(MainWindow), new UIPropertyMetadata(0));

        public static readonly DependencyProperty SkeletonYProperty =
                DependencyProperty.Register("SkeletonY", typeof(int), typeof(MainWindow), new UIPropertyMetadata(0));

        public int TotalPlayers
        {
            get { return (int)GetValue(TotalPlayersProperty); }
            set { SetValue(TotalPlayersProperty, value); }
        }

        public int PedestrianCounts
        {
            get { return (int)GetValue(PedestrianCountsProperty); }
            set { SetValue(PedestrianCountsProperty, value); }
        }

        public int SkeletonX
        {
            get { return (int)GetValue(SkeletonXProperty); }
            set { SetValue(SkeletonXProperty, value); }
        }

        public int SkeletonY
        {
            get { return (int)GetValue(SkeletonYProperty); }
            set { SetValue(SkeletonYProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(drawingGroup);

            Globals.ds = new TrajectoryDbDataSet();
            tw1 = new TrajectoryWindow();

            TotalPlayers = 0;
            PedestrianCounts = 0;

            //Default to open visualiser
            Expander.IsExpanded = true;

            //Start the db timer to update according to the time interval setting
            dbTimer = new Timer(Settings.Default.dbTimerSetting);
            dbTimer.Elapsed += new ElapsedEventHandler(dbTimer_Elapsed);
            dbTimer.Start();

            //Start the xml timer to update to the time interval setting
            xmlTimer = new Timer(Settings.Default.xmlTimerSetting);
            xmlTimer.Elapsed += new ElapsedEventHandler(xmlTimer_Elapsed);
            xmlTimer.Start();

            //Add hook for settings events
            Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);

            ////Restart Management
            ////RecoveryData rd = new RecoveryData(RecoveryCallback, state);
            ////RecoverySettings rs = new RecoverySettings(rd, 1000);
            ////ApplicationRestartRecoveryManager.RegisterForApplicationRecovery(rs);
            ////ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings("restart",RestartRestrictions.None));

            RecoveryHelper.RestartRecoveryHelper<TrajectoryDbDataSet> rrh = new RecoveryHelper.RestartRecoveryHelper<TrajectoryDbDataSet>();
            rrh.CheckForRestart();
            //MessageBox.Show(ds1.trajectories.Count.ToString());

            //Globals.ds = ds1;

            rrh.RegisterForRestartAndRecovery("PedestrianTracker", "Recover", Globals.ds, 50000, RecoveryHelper.FileType.Xml, RecoveryHelper.RestartRestrictions.None);
            
            //Start logging
            Globals.Log("Starting Log");

            loadKinect();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            bool trajectoriesNotEmpty = Globals.ds.trajectories.Rows.Count != 0;

            bool lastDataRowNoMatch = false;

            if (this.lastDataRow == null)
            {
                lastDataRowNoMatch = true;
            }

            else if (this.lastDataRow != Globals.ds.trajectories.Rows[Globals.ds.trajectories.Rows.Count - 1])
            {
                lastDataRowNoMatch = true;
            }


            if (lastDataRowNoMatch && trajectoriesNotEmpty)
            {
                MessageBoxResult result = MessageBox.Show(this, "You have unsaved trajectory data.  If you close this window, all unsaved data will be lost."
                    , "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result != MessageBoxResult.OK)
                {
                    e.Cancel = true;
                }
            }

            tw1.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            updateDatabase();
            writeToXml();

            if (myKinect != null)
            {
                myKinect.Stop();
            }
        }


        // Method to initialize kinect
        private bool loadKinect()
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                //errorMessage += "No Kinects detected ";
                return false;
            }

            myKinect = KinectSensor.KinectSensors[0];

            //Enabling sensor streams with a fixed parameter
            try
            {
                myKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                myKinect.SkeletonStream.Enable();
            }

            catch
            {
                return false;
                //errorMessage += "Unable to enable sensor streams ";
            }

            //Add event handler for streams
            //myKinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(myKinect_AllFramesReady);
            myKinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(myKinect_ColorFrameReady);
            myKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);

            try
            {
                myKinect.Start();
            }
            catch
            {
                return false;
                //errorMessage += "Unable to start Kinect ";
            }

            //if (errorMessage.Length > 0)
            //{
            //    //errorBox.Text = errorMessage;
            //}

            return true;

        }

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if (myKinect.ColorStream.IsEnabled)
            {
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame == null)
                    {
                        return;
                    }

                    byte[] pixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixels);

                    int stride = colorFrame.Width * 4;
                    image1.Source =
                        BitmapSource.Create(colorFrame.Width, colorFrame.Height,
                        96, 96, PixelFormats.Bgr32, null, pixels, stride);
                }
            }
        }

        void myKinect_SkeletonFrameReady(object sencer, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {

                //Make sure skeleton frame is not empty
                if (skeletonFrame == null)
                {
                    return;
                }

                //Create Trajectory Canvases

                if (trajectories == null)
                {
                    this.CreateTrajectoryCanvases();
                }

                //Copy skeleton data
                try
                {
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }

                catch
                {
                    errorMessage += "Unable to copy over skeleton data";
                }

                //Retrieve floor clipping plane parameters
                this.A = skeletonFrame.FloorClipPlane.Item1;
                this.B = skeletonFrame.FloorClipPlane.Item2;
                this.C = skeletonFrame.FloorClipPlane.Item3;
                this.D = skeletonFrame.FloorClipPlane.Item4;

                //Make sure counter starts from 0 every frame
                TotalPlayers = 0;

                int trackedSkeleton = 0;

                ////Make retarded transparent brush so that you can paint on it
                //Brush TransparentBrush = new SolidColorBrush();
                //TransparentBrush.Opacity = 0;
                //dc.DrawRectangle(TransparentBrush, null, new Rect(0, 0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton s in skeletons)
                    {
                        this.trajectoryCanvas = trajectories[trackedSkeleton++];

                        if (s.TrackingState != SkeletonTrackingState.NotTracked)
                        {

                            //If has been tracking for too long 
                            //if (trajectoryCanvas.pointList.Count > 100)
                            //{
                            //    s.TrackingState = SkeletonTrackingState.NotTracked;
                            //    trajectoryCanvas.Reset();
                            //    continue;
                            //}

                            //set tracking state to Position Only and increment player count
                            s.TrackingState = SkeletonTrackingState.PositionOnly;
                            TotalPlayers++;

                            trajectoryCanvas.RefreshTrajectory(s, SkeletonPointToScreen(s.Position), trackedSkeleton);

                        }

                        //Either not tracking yet or leaving tracking state
                        else
                        {
                            //Minimum distance threshold to count those that have been tracked long enough
                            if (trajectoryCanvas.Distance > DistanceThreshold)
                            {
                                PedestrianCounts++;

                                trajectoryCanvas.updateTrajectory();
                                //Debug.WriteLine("number of rows added: " + trajectoryCanvas.updateDatabase());
                            }

                            trajectoryCanvas.Reset();
                        }
                    }

                }
            }

            UpdateFrameRate();
        }


        //void myKinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        //{

        //    if (myKinect.ColorStream.IsEnabled)
        //    {
        //        using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
        //        {
        //            if (colorFrame == null)
        //            {
        //                return;
        //            }

        //            byte[] pixels = new byte[colorFrame.PixelDataLength];
        //            colorFrame.CopyPixelDataTo(pixels);

        //            int stride = colorFrame.Width * 4;
        //            image1.Source =
        //                BitmapSource.Create(colorFrame.Width, colorFrame.Height,
        //                96, 96, PixelFormats.Bgr32, null, pixels, stride);
        //        }
        //    }



        //    using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
        //    {

        //        //Make sure skeleton frame is not empty
        //        if (skeletonFrame == null)
        //        {
        //            return;
        //        }

        //        //Create Trajectory Canvases

        //        if (trajectories == null)
        //        {
        //            this.CreateTrajectoryCanvases();
        //        }

        //        //Copy skeleton data
        //        try
        //        {
        //            skeletonFrame.CopySkeletonDataTo(skeletons);
        //        }

        //        catch
        //        {
        //            errorMessage += "Unable to copy over skeleton data";
        //        }

        //        //Retrieve floor clipping plane parameters
        //        this.A = skeletonFrame.FloorClipPlane.Item1;
        //        this.B = skeletonFrame.FloorClipPlane.Item2;
        //        this.C = skeletonFrame.FloorClipPlane.Item3;
        //        this.D = skeletonFrame.FloorClipPlane.Item4;

        //        //Make sure counter starts from 0 every frame
        //        TotalPlayers = 0;

        //        int trackedSkeleton = 0;

        //        ////Make retarded transparent brush so that you can paint on it
        //        //Brush TransparentBrush = new SolidColorBrush();
        //        //TransparentBrush.Opacity = 0;
        //        //dc.DrawRectangle(TransparentBrush, null, new Rect(0, 0, RenderWidth, RenderHeight));

        //        if (skeletons.Length != 0)
        //        {
        //            foreach (Skeleton s in skeletons)
        //            {
        //                this.trajectoryCanvas = trajectories[trackedSkeleton++];

        //                if (s.TrackingState != SkeletonTrackingState.NotTracked)
        //                {

        //                    //If has been tracking for too long 
        //                    //if (trajectoryCanvas.pointList.Count > 100)
        //                    //{
        //                    //    s.TrackingState = SkeletonTrackingState.NotTracked;
        //                    //    trajectoryCanvas.Reset();
        //                    //    continue;
        //                    //}

        //                    //set tracking state to Position Only and increment player count
        //                    s.TrackingState = SkeletonTrackingState.PositionOnly;
        //                    TotalPlayers++;

        //                    trajectoryCanvas.RefreshTrajectory(s, SkeletonPointToScreen(s.Position), trackedSkeleton);

        //                }

        //                //Either not tracking yet or leaving tracking state
        //                else
        //                {
        //                    //Minimum distance threshold to count those that have been tracked long enough
        //                    if (trajectoryCanvas.Distance > DistanceThreshold)
        //                    {
        //                        PedestrianCounts++;

        //                        trajectoryCanvas.updateTrajectory();
        //                        //Debug.WriteLine("number of rows added: " + trajectoryCanvas.updateDatabase());
        //                    }

        //                    trajectoryCanvas.Reset();
        //                }
        //            }

        //        }
        //    }
        //}

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);


            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            // players contains the player index for each pixel in depthFrame

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency 
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions       
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0;
                depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                depthIndex++, colorIndex += 4)
            {
                //get the player (requires skeleton tracking enabled for values)
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //gets the depth value
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                //.9M or 2.95'
                if (depth <= 900)
                {
                    //we are very close
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;

                }
                // .9M - 2M or 2.95' - 6.56'
                else if (depth > 900 && depth < 2000)
                {
                    //we are a bit further away
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 0;
                }
                // 2M+ or 6.56'+
                else if (depth > 2000)
                {
                    //we are the farthest
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 255;
                }


                ////equal coloring for monochromatic histogram
                //byte intensity = CalculateIntensityFromDepth(depth);
                //pixels[colorIndex + BlueIndex] = intensity;
                //pixels[colorIndex + GreenIndex] = intensity;
                //pixels[colorIndex + RedIndex] = intensity;


                //Color all players "gold"
                if (player > 0)
                {
                    pixels[colorIndex + BlueIndex] = Colors.Gold.B;
                    pixels[colorIndex + GreenIndex] = Colors.Gold.G;
                    pixels[colorIndex + RedIndex] = Colors.Gold.R;
                }

            }

            return pixels;
        }

        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = myKinect.MapSkeletonPointToDepth(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        private Point SkeletonPointToPoint(float X, float Y, float Z)
        {
            SkeletonPoint skelPoint = new SkeletonPoint();
            skelPoint.X = X;
            skelPoint.Y = Y;
            skelPoint.Z = Z;

            ColorImagePoint imgPoint = myKinect.MapSkeletonPointToColor(skelPoint, ColorImageFormat.RgbResolution640x480Fps30);

            return new Point(imgPoint.X, imgPoint.Y);
        }

        private void DrawCenterPoint(Skeleton s, DrawingContext dc)
        {
            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            SkeletonPointToScreen(s.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
        }

        private void CreateTrajectoryCanvases()
        {
            this.trajectories = new List<Trajectory>
                    {
                        this.trajectoryCanvas1,
                        this.trajectoryCanvas2,
                        this.trajectoryCanvas3,
                        this.trajectoryCanvas4,
                        this.trajectoryCanvas5,
                        this.trajectoryCanvas6
                    };
        }

        private int[] updateDatabase()
        {
            int[] result = new int[2];

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //update trajectories
                using (TrajectoryDbDataSetTableAdapters.trajectoriesTableAdapter trajectoriesDa = new TrajectoryDbDataSetTableAdapters.trajectoriesTableAdapter())
                {

                    if (Globals.ds != null)
                    {
                        result[0] = trajectoriesDa.Update(Globals.ds);
                    }

                    using (TrajectoryDbDataSetTableAdapters.pointsTableAdapter pointsDa = new TrajectoryDbDataSetTableAdapters.pointsTableAdapter())
                    {

                        if (Globals.ds != null)
                        {

                            result[1] = pointsDa.Update(Globals.ds);

                        }
                    }

                }
            }

            return result;
        }

        private void writeToXml()
        {
            //Check if there is already a directory for saving trajectory data
            string xmlfilepath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Pedestrian Tracker Data";

            if (!Directory.Exists(xmlfilepath))
            {
                Directory.CreateDirectory(xmlfilepath);
            }
            
            this.xmlfilename = string.Format("pedestriandata-{0:yyyy-MM-dd_hh-mm-ss}.xml",DateTime.Now);

            using (StreamWriter xmlSW = new StreamWriter(xmlfilepath + "\\" + xmlfilename))
            {
                Globals.ds.WriteXml(xmlSW, XmlWriteMode.WriteSchema);
            }
        }

        /// <summary>
        /// Drawing helper methods
        /// </summary>

        //private void drawRoadAxis()
        //{
        //    Point firstPoint;

        //    try
        //    {
        //        firstPoint = SkeletonPointToPoint(-0.5f, 0.0f, 1f);
        //    }

        //    catch (Exception e)
        //    {
        //        return;
        //    }

        //    Point secondPoint = SkeletonPointToPoint(1, 0, 1);

        //    Line line = new Line();
        //    line.X1 = firstPoint.X;
        //    line.Y1 = firstPoint.Y;
        //    line.X2 = secondPoint.X;
        //    line.Y2 = secondPoint.Y;
        //    line.Stroke = Brushes.Red;
        //    line.StrokeThickness = 4;

        //    C1.Children.Add(line);

        //    //drawingContext.DrawLine(new Pen(Brushes.Black,2),firstPoint, secondPoint);

        //}

        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.Red)
                {
                    Opacity = 0.3
                });
            GeometryModel3D model = new GeometryModel3D(
                mesh, material);
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }
        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v1, v0);
        }

        private double FindYPoint(double X, double Z)
        {
            try
            {
                return (-D - A * X - C * Z) / B;
            }
            catch (Exception e)
            {
                return 0.0;
            }
        }

        private void DrawClippingPlane()
        {
            Model3DGroup plane = new Model3DGroup();

            Point3D p0 = new Point3D(-planeWidth, FindYPoint(-planeWidth, -planeDepth), -planeDepth);
            Point3D p1 = new Point3D(planeWidth, FindYPoint(planeWidth, -planeDepth), -planeDepth);
            Point3D p2 = new Point3D(-planeWidth, FindYPoint(-planeWidth, 0), 0);
            Point3D p3 = new Point3D(planeWidth, FindYPoint(planeWidth, 0), 0);

            plane.Children.Add(CreateTriangleModel(p0, p1, p3));
            plane.Children.Add(CreateTriangleModel(p1, p2, p3));

            planeModel = new ModelVisual3D();
            planeModel.Content = plane;
            viewport.Children.Add(planeModel);
        }

        private void DrawRoadAxis()
        {
            Point3DCollection points = new Point3DCollection();

            points.Add(new Point3D(-planeWidth, FindYPoint(-planeWidth, -10), -10));
            points.Add(new Point3D(planeWidth, FindYPoint(planeWidth, -10), -10));

            line = new WireLines()
            {
                Lines = points,
                Color = Colors.Black,
                Thickness = 4.0,
            };

            line.Transform = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), Settings.Default.KinectAngle),
                CenterX = 0,
                CenterY = 0,
                CenterZ = -10
            };

            model.Children.Add(line);
        }

        protected void UpdateFrameRate()
        {

                ++this.TotalFrames;

                DateTime cur = DateTime.Now;
                var span = cur.Subtract(this.lastTime);
                if (this.lastTime == DateTime.MaxValue || span >= TimeSpan.FromSeconds(1))
                {
                    // A straight cast will truncate the value, leading to chronic under-reporting of framerate.
                    // rounding yields a more balanced result
                    this.FrameRate = (int)Math.Round((this.TotalFrames - this.LastFrames) / span.TotalSeconds);
                    this.LastFrames = this.TotalFrames;
                    this.lastTime = cur;
                }
        }
        /// <summary>
        /// Menu event handlers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mnuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            openFileDialog1.Title = "Open Trajectory Data";
            openFileDialog1.DefaultExt = ".csv";
            openFileDialog1.Filter = "CSV files (*.csv)|*.txt|All files (*.*)|*.*";
            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result == true)
            {
                string openFileName = openFileDialog1.FileName;

                using (StreamReader sr = new StreamReader(openFileName))
                {
                    try
                    {

                        string line;
                        string[] row;
                        int i = 0;

                        while ((line = sr.ReadLine()) != null)
                        {
                            //Do not read the header
                            if (i > 0)
                            {
                                row = line.Split(',');
                                Globals.ds.trajectories.Rows.Add(row);
                            }

                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void mnuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog1.Title = "Save Trajectory Data";
            saveFileDialog1.DefaultExt = ".csv";
            saveFileDialog1.Filter = "CSV files (*.csv)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FileName = "trajectories.csv";

            Nullable<bool> result = saveFileDialog1.ShowDialog();

            if (result == true)
            {
                string saveFileName = saveFileDialog1.FileName;

                try
                {
                    // Create the CSV file to which grid data will be exported.

                    StreamWriter sw = new StreamWriter(saveFileName, false);
                    // First we will write the headers.
                    DataTable dt = Globals.ds.trajectories;
                    int iColCount = dt.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        sw.Write(dt.Columns[i]);
                        if (i < iColCount - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);

                    // Now write all the rows.

                    //foreach (DataRow dr in dt.Rows)
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        DataRow dr = dt.Rows[j];

                        //Save last row
                        if (j == dt.Rows.Count - 1)
                        {
                            this.lastDataRow = dr;
                        }

                        for (int i = 0; i < iColCount; i++)
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {
                                sw.Write(dr[i].ToString());
                            }
                            if (i < iColCount - 1)
                            {
                                sw.Write(",");
                            }
                        }

                        sw.Write(sw.NewLine);
                    }
                    sw.Close();

                    MessageBox.Show("Trajectories succesfully saved to CSV file");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void mnuViewShowTrackedData_Clicked(object sender, RoutedEventArgs e)
        {
            if (tw1 == null)
            {
                tw1 = new TrajectoryWindow();
            }

            tw1.Show();

        }

        private void mnuViewShowPastTrajectories_Checked(object sender, RoutedEventArgs e)
        {
            dv = new DrawingGroup();
            Brush TransparentBrush = new SolidColorBrush();
            TransparentBrush.Opacity = 0;
            PathFigureCollection trajectoryPathFigureCollection = new PathFigureCollection();

            foreach (TrajectoryDbDataSet.trajectoriesRow row in Globals.ds.trajectories.Rows)
            {
                TrajectoryDbDataSet.pointsRow[] pointsRows = (TrajectoryDbDataSet.pointsRow[])Globals.ds.points.Select("t_id = " + row.t_id);

                PathSegmentCollection segs = new PathSegmentCollection();

                try
                {
                    Point firstPoint = SkeletonPointToPoint((float)pointsRows[0].X, (float)pointsRows[0].Y, (float)pointsRows[0].Z);

                    foreach (TrajectoryDbDataSet.pointsRow pointRow in pointsRows)
                    {
                        Point point = SkeletonPointToPoint((float)pointRow.X, (float)pointRow.Y, (float)pointRow.Z);

                        segs.Add(new LineSegment(point, true));
                    }

                    PathFigure fig = new PathFigure(firstPoint, segs, false);
                    trajectoryPathFigureCollection.Add(fig);
                }

                catch
                {
                    return;
                }
            }

            PathGeometry geometry = new PathGeometry(trajectoryPathFigureCollection);
            GeometryDrawing gd = new GeometryDrawing();
            gd.Geometry = geometry;
            gd.Pen = new Pen(Brushes.Red, 0.5);

            pastTrajectories.Source = new DrawingImage(gd);

            //using (DrawingContext drawingContext = dv.Open())
            //{
            //    drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, 0, RenderWidth, RenderHeight));
            //    drawingContext.DrawGeometry(null, new Pen(Brushes.Red, 2), geometry);
            //}

        }

        private void mnuViewShowPastTrajectories_Unchecked(object sender, RoutedEventArgs e)
        {
            pastTrajectories.Source = null;
        }

        private void mnuViewOptions_Clicked(object sender, RoutedEventArgs e)
        {
            ops = new Options();
            ops.Show();
        }

        private void mnuViewShowRoadAxis_Unchecked(object sender, RoutedEventArgs e)
        {
            //RoadAxis.Visibility = Visibility.Hidden;
            if (line != null)
            {
                if (model.Children.Contains(line))
                {
                    model.Children.Remove(line);
                    line = null;
                }

                if (viewport.Children.Contains(planeModel))
                {
                    viewport.Children.Remove(planeModel);
                    planeModel = null;
                }

                if (model.Children.Contains(axes))
                {
                    model.Children.Remove(axes);
                    axes = null;
                }
            }
        }

        private void mnuViewShowRoadAxis_Checked(object sender, RoutedEventArgs e)
        {
            axes = new Axes()
            {
                ArrowEnds = Petzold.Media2D.ArrowEnds.End,
                Color = Colors.White,
                Thickness = 3,
            };

            model.Children.Add(axes);

            DrawClippingPlane();
            DrawRoadAxis();

        }

        private void mnuViewShow3DTrajectory_Clicked(object sender, RoutedEventArgs e)
        {
            Trajectory3DView traj3d = new Trajectory3DView();
            traj3d.Show();
        }

        //When the database timer ends, update the datasource
        private void dbTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Globals.Log("Updating database");
            updateDatabase();
            
        }

        private void xmlTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Globals.Log("Writing an xml file");
            writeToXml();

        }

        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            if (line != null)
            {
                line.Transform = new RotateTransform3D()
                {
                    Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), Settings.Default.KinectAngle),
                    CenterX = 0,
                    CenterY = 0,
                    CenterZ = -10
                };
            }
        }

        private void CollapseVisualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (myKinect.ColorStream.IsEnabled)
            {
                C1.Visibility = Visibility.Collapsed;
                myKinect.ColorStream.Disable();
            }
            else
            {
                C1.Visibility = Visibility.Visible;
                myKinect.ColorStream.Enable();
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (myKinect != null)
            {
                myKinect.ColorStream.Enable();
                //Enable drawing on each trajectory canvas
                foreach (Trajectory trajectoryCanvas in trajectories)
                {
                    trajectoryCanvas.shouldDraw = true;
                }
                //Expand window
                this.Height = 650;
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            myKinect.ColorStream.Disable();
            
            //Disable drawing on each trajectory canvas
            foreach (Trajectory trajectoryCanvas in trajectories)
            {
                trajectoryCanvas.shouldDraw = false;
            }

            //Contract window
            this.Height = 130;
        }

        //Recovery callback method
        private int RecoveryCallback(object state)
        {
            Timer pinger = new Timer(4000);
            pinger.Elapsed += new ElapsedEventHandler(PingSystem);
            pinger.Enabled = true;

            writeToXml();

            //MessageBox.Show("Recovered");

            ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(true);

            return 0;
        }

        //Method called to ping the WER that the recovery procedure is in progress
        private void PingSystem(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Ping");
            ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();
        }


        private void mnuCrash_Click(object sender, RoutedEventArgs e)
        {
            //Environment.FailFast("Uh oh! Looks like a crash");
        }

        protected void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
