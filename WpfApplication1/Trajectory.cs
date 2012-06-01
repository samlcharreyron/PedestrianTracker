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

namespace PedestrianTracker
{
    public class Trajectory : UserControl
    {
        private Skeleton currentSkeleton;
        private int trackedSkeleton;
        private Point currentPoint;
        private SkeletonPoint lastPoint;
        private List<Point> pointList;

        //For drawing the trajectory
        private PathSegmentCollection trajectoryPathSegments;
        private PathFigure trajectoryPathFigure;
        private PathFigureCollection trajectoryPathFigureCollection;
        private PathGeometry trajectoryPathGeometry;

        private FormattedText trajectoryText;
        
        //Brushes
        private Brush TrajectoryBrush;
        private readonly Brush centerPointBrush = Brushes.Black;
        private readonly Brush TrajectoryTextBrush = Brushes.Gray;

        //Drawing Constants
        private const double BodyCenterThickness = 8;
        private const double textOffset = 10;

        //Saving data
        private SqlConnection connection;
        private string tableName;
        //private TrajectoryDbDataSet ds;
        private SqlDataAdapter da;
        private SqlCommandBuilder cmdBuilder;
        private TrajectoryDbDataSet.trajectoriesRow t_key;
        private int t_id;

        private int frameIteration = 0;
        private double deltaDistance = 0;
        public double velocity  = 0;
        private float deltaX = 0;
        public string Direction = "N";

        //Averages
        private double averageVelocity = 0;
        private List<int> directionSum = new List<int>(); 
        private string averageDirection = "N";

  
        private readonly string filepath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
   
        public string Name
        {
            get;
            set;
        }

        public double Distance
        {
            get;
            set;
        }

        public void Reset()
        {
            this.currentSkeleton = null;
            this.pointList = null;
            this.trajectoryPathSegments = null;
            this.trajectoryPathFigure = null;
            this.trajectoryPathFigureCollection = null;
            this.trajectoryPathGeometry = null;
            this.Distance = 0.0;
            this.velocity = 0.0;
            this.Direction = "NA";

            InvalidateVisual();
        }

        public void AddPoint(Point point)
        {
            try
            {
                if (this.pointList == null)
                {
                    this.pointList = new List<Point>();
                }

                this.pointList.Add(point);
                this.AddSegment(point);
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

        private void IncrementDistance(SkeletonPoint lastPoint, SkeletonPoint thisPoint)
        {
            //Implement subsampling of distance to every 10 frames

            if (this.frameIteration == 10)
            {
                frameIteration = 0;

                try
                {
                    deltaX = thisPoint.X - lastPoint.X;

                    Direction = deltaX > 0 ? "R" : "L";

                    //Average Direction
                    directionSum.Add(Direction.Equals("R") ? 1 : 0);

                    deltaDistance = Math.Sqrt(Math.Pow(((double)thisPoint.X - (double)lastPoint.X), 2.0)
                                                        + Math.Pow(((double)thisPoint.Y - (double)lastPoint.Y), 2.0)
                                                        + Math.Pow(((double)thisPoint.Z - (double)lastPoint.Z), 2.0));

                    if (deltaDistance > 0.01)
                    {
                        Distance += deltaDistance;
                        velocity = deltaDistance * 10;

                        //load data first
                        if (Globals.ds == null)
                        {
                            Globals.ds = new TrajectoryDbDataSet();
                        }

                        addPoint(thisPoint, Distance, deltaDistance, velocity, Direction, t_key);

                        //addRow(thisPoint, Distance, deltaDistance, velocity, Direction);
                    }
                }
                catch
                {
                    return;
                }
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
            this.trajectoryPathFigure = new PathFigure(pointList.First(), trajectoryPathSegments, false);
            this.trajectoryPathFigureCollection = new PathFigureCollection();
            this.trajectoryPathFigureCollection.Add(trajectoryPathFigure);
            this.trajectoryPathGeometry = new PathGeometry(trajectoryPathFigureCollection);

            //At first point add trajectory row to dataset
            if (pointList.Count == 1)
            {
                addTrajectory();
            }
            
            //if only one point do not calculate the distance
            if (pointList.Count > 1)
            {
                IncrementDistance(lastPoint, currentSkeleton.Position);
            }

            lastPoint = currentSkeleton.Position;

            this.InvalidateVisual();
        }

        //Private saving data

        //public void LoadData()
        //{
        //    using (connection = new SqlConnection(MainWindow.connectionString))
        //    {
        //        connection.Open();

        //        this.tableName = "Trajectory" + trackedSkeleton;

        //        using (da = new SqlDataAdapter(String.Format("Select * from {0}", tableName), MainWindow.connectionString))
        //        {
        //            this.ds = new TrajectoryDbDataSet();
        //            this.cmdBuilder = new SqlCommandBuilder(da);
        //            da.Fill(ds, tableName);
        //        }
        //    }
        //}

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
        public void addPoint(SkeletonPoint point, double distance, double deltaDistance, double velocity, string direction, TrajectoryDbDataSet.trajectoriesRow t_key)
        {
            try
            {
                Globals.ds.points.AddpointsRow(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction, (byte) trackedSkeleton, t_key);
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
                this.t_key =  Globals.ds.trajectories.AddtrajectoriesRow((byte)trackedSkeleton, DateTime.Now, DateTime.Now, 0, "N", 0);

                //Store this trajectory's row primary key
                this.t_id = t_key.t_id;
            }
            catch (Exception e)
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
                currentRow.end_time = DateTime.Now;

                //Average velocity and direction
                double velocitySum = 0;
                int directionSum = 0;
                int rows = 0;

                foreach (DataRow row in Globals.ds.points.Select(String.Format("t_id = {0}", t_id)))
                {
                    velocitySum += (double)row[5];
                    directionSum += Direction.Equals("R") ? 1 : 0;
                    
                    rows++;
                }

                currentRow.average_velocity = velocitySum/rows;
                currentRow.average_direction = (directionSum/rows > 0.5) ? "R" : "L";
                currentRow.length = this.Distance;

            }

            catch (Exception e)
            {
                return;
            }
        }


        //public void addRow(SkeletonPoint point, double distance, double deltaDistance, double velocity, string direction)
        //{
        //    try
        //    {
        //        switch (trackedSkeleton)
        //        {
        //            case 1:
        //                ds.Trajectory1.AddTrajectory1Row(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction);
        //                break;
        //            case 2:
        //                ds.Trajectory2.AddTrajectory2Row(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction);
        //                break;
        //            case 3:
        //                ds.Trajectory3.AddTrajectory3Row(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction);
        //                break;
        //            case 4:
        //                ds.Trajectory4.AddTrajectory4Row(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction);
        //                break;
        //            case 5:
        //                ds.Trajectory5.AddTrajectory5Row(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction);
        //                break;
        //            case 6:
        //                ds.Trajectory6.AddTrajectory6Row(point.X, point.Y, point.Z, distance, deltaDistance, velocity, direction);
        //                break;
        //        }
        //    }
        //    catch
        //    {
        //        return;
        //    }
        //}

        //public int updateDatabase()
        //{
        //    using (connection = new SqlConnection(MainWindow.connectionString))
        //    {
        //        connection.Open();

        //        this.tableName = "Trajectory" + trackedSkeleton;

        //        using (da = new SqlDataAdapter(String.Format("Select * from {0}", tableName), MainWindow.connectionString))
        //        {
        //            this.cmdBuilder = new SqlCommandBuilder(da);

        //            if (ds != null)
        //            {
        //                return da.Update(ds, tableName);
        //            }
        //            else return 0;
        //        }
        //    }
        //}


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (currentSkeleton != null && currentSkeleton.TrackingState == SkeletonTrackingState.PositionOnly)
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

                    trajectoryText = new FormattedText("Skeleton " + trackedSkeleton + "\nvelocity: " + this.velocity.ToString("#.##" + " m/s") + "\nDirection: " + this.Direction,
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
            if (trajectoryPathGeometry != null && currentSkeleton != null & currentSkeleton.TrackingState!=SkeletonTrackingState.NotTracked)
            {
                drawingContext.DrawGeometry(null, new Pen(this.TrajectoryBrush,2), this.trajectoryPathGeometry);
            }
        }

    }
}
