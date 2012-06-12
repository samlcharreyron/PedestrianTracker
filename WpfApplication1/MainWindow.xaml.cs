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
using Microsoft.Kinect;

//For testing
using System.Diagnostics;

namespace PedestrianTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
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
        

        //Drawing
        private readonly Brush centerPointBrush = Brushes.Black;
        private const double BodyCenterThickness = 8;

        Skeleton[] skeletons = new Skeleton[numberOfSkeletons];

        private List<Trajectory> trajectories;

        private Trajectory trajectoryCanvas;

        private DrawingGroup dv;

        //Database stuff
        private System.Data.SqlClient.SqlConnection connection;
        public const string connectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\TrajectoryDb.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";

        //Dataset stuff
        private DataRow lastDataRow;
        
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

            //SkeletonImage.Source = this.imageSource;

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
            myKinect.Stop();
        }


        // Method to initialize kinect
        private bool loadKinect()
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                errorMessage += "No Kinects detected ";
                return false;
            }

            myKinect = KinectSensor.KinectSensors[0];

            //Enabling sensor streams with a fixed parameter
            try
            {
                myKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                //myKinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                myKinect.SkeletonStream.Enable();
            }

            catch
            {
                errorMessage += "Unable to enable sensor streams ";
            }

            //Add event handler for streams
            myKinect.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(myKinect_AllFramesReady);

            try
            {
                myKinect.Start();
            }
            catch
            {
                errorMessage += "Unable to start Kinect ";
            }

            if (errorMessage.Length > 0)
            {
                //errorBox.Text = errorMessage;
            }

            return true;

        }

        void myKinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            //{
            //    //Make sure that the depthFrame if empty
            //    if (depthFrame == null)
            //    {
            //        return;
            //    }

            //    byte[] pixels = GenerateColoredBytes(depthFrame);

            //    //number of bytes per row width * 4 (B,G,R,Empty)
            //    int stride = depthFrame.Width * 4;

            //    //create image
            //    image1.Source =
            //        BitmapSource.Create(depthFrame.Width, depthFrame.Height,
            //        96, 96, PixelFormats.Bgr32, null, pixels, stride);
            //}

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
        }

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

            return new Point(imgPoint.X ,imgPoint.Y);
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
                            
                            result [1] = pointsDa.Update(Globals.ds);
                            
                        }
                    }

                }
            }

            return result;
        }


        /*
         * Menu Event Handlers
         * 
         */

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

    }
}
