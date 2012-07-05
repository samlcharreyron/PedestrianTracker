using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
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
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Kinect;
using PedestrianTracker.Properties;

namespace PedestrianTracker
{
    /// <summary>
    /// The Trajectory class is a user defined control and contains variables and methods that define and 
    /// are applied to a moving skeleton. There are a total of 6 instances of Trajectory that are initialized 
    /// each corresponding to a possible tracked skeleton.  The Trajectory class determines the position, velocity, direction
    /// of the tracked skeleton, overlays this info on the RGB view, and stores it in the dataset.
    /// </summary>

    public class Trajectory : UserControl
    {
        private Skeleton currentSkeleton;
        private int trackedSkeleton;
        private Point currentPoint;
        private SkeletonPoint lastPoint;
        public List<Point> pointList;

        ////For measuring time
        //private DateTime lastTime, currentTime;
        //private int deltaTime;

        //Frame subsampling for trajectory data MUST BE AN INTEGER MULTIPLE OF 30 (Frame Rate)
        private int FrameSub = 2;

        //Thresholds for filtering out eroneous trajectory points
        private const double MinDeltaDistance = 0.005;
        private const double MaxVelocity = 3.5;
        private const double DeltaYThreshold = 0.022;
        private const double DeltaZThreshold = 0.022;
        private const double VelocityLowThreshold = 0.3;
        private const double VelocityHighThreshold = 0.3;

        //The angle in degrees between the road and the kinect heading (0 if tracking horizontally), 90 if tracking head on)
        private int TrackingAngle = 0;
        private float AngleProjectionFactorX = 1;
        private float AngleProjectionFactorZ = 0;
        private float deltaY = 0;
        private float deltaZ = 0;

        //For drawing the trajectory
        private PathSegmentCollection trajectoryPathSegments;
        private PathFigure trajectoryPathFigure;
        private PathFigureCollection trajectoryPathFigureCollection;
        private PathGeometry trajectoryPathGeometry;
        private FormattedText trajectoryText;

        //Enables drawing functionality
        public bool shouldDraw = true;
        
        //Brushes
        private Brush TrajectoryBrush = Brushes.Red;
        private readonly Brush centerPointBrush = Brushes.Black;
        private readonly Brush TrajectoryTextBrush = Brushes.Gray;

        //Drawing Constants
        private const double BodyCenterThickness = 8;
        private const double textOffset = 10;

        //Saving data
        private SqlConnection connection;
        private SqlDataAdapter da;
        private SqlCommandBuilder cmdBuilder;
        private TrajectoryDbDataSet.trajectoriesRow t_key;
        private int t_id;
        private DateTime startTime;

        private int frameIteration = 0;
        private double deltaDistance = 0;
        public double velocity  = 0;
        private float deltaP = 0;
        public string Direction = "N";

        private int milliseconds;
        private int lastmilliseconds;

        public double Distance
        {
            get;
            set;
        }

        // Called when skeleton is not being tracked
        public void Reset()
        {
            this.currentSkeleton = null;
            this.lastPoint = new SkeletonPoint();
            this.pointList = new List<Point>();
            this.trajectoryPathSegments = null;
            this.trajectoryPathFigure = null;
            this.trajectoryPathFigureCollection = null;
            this.trajectoryPathGeometry = null;
            this.Distance = 0.0;
            this.velocity = 0.0;
            this.Direction = "NA";
            this.TrackingAngle = Properties.Settings.Default.KinectAngle;

            if (Settings.Default.UseAngle)
            {
                this.AngleProjectionFactorX = (float)Math.Cos((TrackingAngle * Math.PI) / 180);
                this.AngleProjectionFactorZ = (float)Math.Sin((TrackingAngle * Math.PI) / 180);
            }
            else
            {
                this.AngleProjectionFactorX = 1;
                this.AngleProjectionFactorZ = 1;
            }

            this.FrameSub = Properties.Settings.Default.TrajectorySubsample;
            this.milliseconds = 0;
            this.lastmilliseconds = 0;
            InvalidateVisual();
        }

        //Adds a 2D point to the list and a segment to the current path segment
        public void AddPoint(Point point)
        {

            try
            {
                if (this.pointList == null)
                {
                    this.pointList = new List<Point>();
                }

                this.pointList.Add(point);
                
                if (shouldDraw)
                {
                    this.AddSegment(point);
                }
            }

            catch
            {
                return;
            }
        }

        public void ClearPoints()
        {
            try
            {
                this.pointList.Clear();
            }
            catch
            {
                return;
            }
        }

        //Adds a segment to the trajectory figure
        private void AddSegment(Point point)
        {
            if (this.trajectoryPathSegments == null)
            {
                this.trajectoryPathSegments = new PathSegmentCollection();
            }
            
            this.trajectoryPathSegments.Add(new LineSegment(point, true));
        }

        private void ClearSegments()
        {
            try
            {
                this.trajectoryPathSegments.Clear();
            }
            catch
            {
                return;
            }
        }


        private float VectorToDistance(float X, float Z)
        {
            return (X * AngleProjectionFactorX + Z * AngleProjectionFactorZ);
        }

        private float EuclideanDistance(float X, float Z)
        {
            return (float) Math.Sqrt(X * X + Z * Z);
        }

        private float[] VectorRotation(float X, float Z)
        {
            return new float[] {X * AngleProjectionFactorX , Z * AngleProjectionFactorZ};
        }

        //Called to update the trajectory data
        private void IncrementDistance(SkeletonPoint thisPoint)
        {
            
            //Implements subsampling of order FrameSub.  For example, for FrameSub = 10, 
            //trajectory data is sampled every 10 frames so 3 times per second.
            if (this.frameIteration == FrameSub)
            {
                frameIteration = 0;
                
                //Makes sure that there are at least 2 points so that distance differentials can be computed
                if (lastPoint != null && (lastPoint.X != 0 && lastPoint.Y != 0 && lastPoint.Z != 0))
                {
                    deltaY = Math.Abs(thisPoint.Y - lastPoint.Y);
                    deltaZ = Math.Abs(thisPoint.Z - lastPoint.Z);
                    
                    //Euclidean distance along the road axis
                    deltaP = EuclideanDistance(thisPoint.X-lastPoint.X,thisPoint.Z-lastPoint.Z);

                    Direction = deltaP > 0 ? "R" : "L";

                    deltaDistance = Math.Abs(deltaP);
                    
                    //Time since trajectory started
                    milliseconds = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;

                    //velocity = deltaDistance * (30 / FrameSub);
                    velocity = deltaDistance * 1000 / (milliseconds - lastmilliseconds);

                    //filters out erroneous data
                    if (deltaDistance > MinDeltaDistance)
                    {
                        Distance += deltaDistance;

                        //load data first
                        if (Globals.ds == null)
                        {
                            Globals.ds = new TrajectoryDbDataSet();
                        }

                        //Adds a point to the Point dataset
                        addPointData(thisPoint, Distance, deltaDistance, velocity, Direction, t_key,milliseconds);

                    }

                }

                //stores the point for the next calculation
                lastPoint = thisPoint;
                lastmilliseconds = milliseconds;
            }

            else frameIteration++;
        }

        // Call every frame refresh
        public void RefreshTrajectory(Skeleton skeleton, Point center, int trackedSkeleton)
        {
            this.currentSkeleton = skeleton;
            this.trackedSkeleton = trackedSkeleton;
            this.currentPoint = center;

            this.AddPoint(currentPoint);

            switch (trackedSkeleton)
            {
                case 1:
                    this.TrajectoryBrush = Brushes.White;
                    break;
                case 2:
                    this.TrajectoryBrush = Brushes.Salmon;
                    break;
                case 3:
                    this.TrajectoryBrush = Brushes.YellowGreen;
                    break;
                case 4:
                    this.TrajectoryBrush = Brushes.Orange;
                    break;
                case 5:
                    this.TrajectoryBrush = Brushes.Purple;
                    break;
                case 6:
                    this.TrajectoryBrush = Brushes.Yellow;
                    break;
                default:
                    this.TrajectoryBrush = Brushes.Pink;
                    break;
            }

            //Draw the trajectory
            if (shouldDraw)
            {
                this.trajectoryPathFigure = new PathFigure(pointList.First(), trajectoryPathSegments, false);
                this.trajectoryPathFigureCollection = new PathFigureCollection();
                this.trajectoryPathFigureCollection.Add(trajectoryPathFigure);
                this.trajectoryPathGeometry = new PathGeometry(trajectoryPathFigureCollection);
            }
            

            //At first point add trajectory row to dataset
            if (pointList.Count == 1)
            {
                addTrajectory();
            }
            
            //if only one point do not calculate the distance
            if (pointList.Count > 0)
            {
                IncrementDistance(currentSkeleton.Position);
            }

            this.InvalidateVisual();
        }

        public static double StandardDeviation(List<double> valueList)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in valueList)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public void loadPointData()
        {
            using (connection = new SqlConnection(MainWindow.connectionString))
            {
                connection.Open();

                using (da = new SqlDataAdapter("points", MainWindow.connectionString))
                {
                    Globals.ds = new TrajectoryDbDataSet();
                    this.cmdBuilder = new SqlCommandBuilder(da);
                    da.Fill(Globals.ds, "points");
                }
            }
        }

        // Add one point to points table, must refer to a trajectory in trajectories table
        public void addPointData(SkeletonPoint point, double distance, double deltaDistance, double velocity, string direction, TrajectoryDbDataSet.trajectoriesRow t_key,int milliseconds)
        {
            try
            {
                Globals.ds.points.AddpointsRow(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction, (byte) trackedSkeleton, t_key,milliseconds);
            }
            catch
            {
                return;
            }
        }


        // When starting a new trajectory, call this.  It should add a new row to the trajectories dataset with junk data that will be updated once trajectory is over
        public void addTrajectory()
        {
            if (Globals.ds == null)
            {
                Globals.ds = new TrajectoryDbDataSet();
            }

            try
            {
                this.startTime = DateTime.Now;
                this.t_key = Globals.ds.trajectories.AddtrajectoriesRow((byte)trackedSkeleton, startTime, startTime, 0, "N", 0);

                //Store this trajectory's row primary key
                this.t_id = t_key.t_id;
            }
            catch
            {
                return;
            }
        }

        //At the end of a trajectory, this updates the row with final info
        public void updateTrajectory()
        {
            try
            {
                TrajectoryDbDataSet.trajectoriesRow currentRow = Globals.ds.trajectories.FindByt_id(this.t_id);
                
                //update row with final values
                //currentRow.end_time = DateTime.Now;

                //Average velocity and direction
                double velocitySum = 0;
                int directionSum = 0;
                int rows = 0;
                List<double> velocities = new List<double>();

                TrajectoryDbDataSet.pointsRow[] pointrows =  Globals.ds.points.Select(String.Format("t_id = {0}", t_id)) as TrajectoryDbDataSet.pointsRow[];

                foreach (TrajectoryDbDataSet.pointsRow row in pointrows)
                {
                    velocities.Add((double)row[5]);
                    //velocitySum += (double)row[5];
                    directionSum += Direction.Equals("R") ? 1 : 0;
                    
                    rows++;

                    if (rows == pointrows.Count())
                    {
                        currentRow.end_time = currentRow.start_time.AddMilliseconds(row.milliseconds);
                    }
                }


                ///<summary>
                ///Filtering out outliers. Find the standard deviation of all velocity values.  If values lie outside of two standard
                ///deviations they are ignored from the average velocity calculation.
                double mean = velocities.Average();
                double stdev = StandardDeviation(velocities);
                int n = 0;

                foreach (double v in velocities)
                {
                    if (v > (mean - 2* stdev) && v < (mean + 2*stdev))
                    {
                        velocitySum += v;
                        n++;
                    }
                }
                
                double tester = velocitySum/n;

                currentRow.average_velocity = (tester.CompareTo(double.NaN)>0) ? tester: mean;
                currentRow.average_direction = (directionSum/rows > 0.5) ? "R" : "L";
                currentRow.length = this.Distance;

            }

            catch (Exception e)
            {
                return;
            }
        }

        //Called everytime the Trajectory instance is rendered (once per frame). Will draw the trajectory, trajectory info box, skeleton center point
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (currentSkeleton != null && currentSkeleton.TrackingState == SkeletonTrackingState.PositionOnly && shouldDraw)
            {
                drawingContext.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.currentPoint,
                            BodyCenterThickness,
                            BodyCenterThickness);

                try
                {
                    //TrajectoryTextBrush.Opacity = .5;

                    trajectoryText = new FormattedText("Skeleton " + trackedSkeleton + "\nvelocity: " + this.velocity.ToString("#.##" + " m/s")  + "\nDirection: " + this.Direction
                        + "\nDistance: " + this.Distance,
                        //+"\ndY: " + deltaY + "  dZ: " + deltaZ,
                        //+ "\nX: " + currentSkeleton.Position.X + "  Y: " + currentSkeleton.Position.Y + "   Z: " + currentSkeleton.Position.Z,
                                                CultureInfo.GetCultureInfo("en-us"),
                                                FlowDirection.LeftToRight,
                                                new Typeface("Verdana"),
                                                12,
                                                this.TrajectoryBrush);

                    drawingContext.DrawRoundedRectangle(TrajectoryTextBrush, 
                                                            null, 
                                                            new Rect(currentPoint.X + textOffset, currentPoint.Y + textOffset, trajectoryText.Width, trajectoryText.Height),
                                                            1, 
                                                            1);

                    drawingContext.DrawText(trajectoryText,
                                                new Point(currentPoint.X + textOffset, currentPoint.Y + textOffset));
                   
                }
                catch
                {
                    return;
                }
            }
            
            //To draw trajectory only if there is something to draw and if the skeleton is tracked
            if (trajectoryPathGeometry != null && currentSkeleton != null && currentSkeleton.TrackingState!=SkeletonTrackingState.NotTracked)
            {
                drawingContext.DrawGeometry(null, new Pen(this.TrajectoryBrush,2), this.trajectoryPathGeometry);
            }
            
        }

    }
}
