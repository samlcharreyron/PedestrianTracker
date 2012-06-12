﻿using System;
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
        private const int FrameSub = 5;

        //Thresholds for filtering out eroneous trajectory points
        private const double MinDeltaDistance = 0.005;
        private const double MaxVelocity = 3.5;

        //For drawing the trajectory
        private PathSegmentCollection trajectoryPathSegments;
        private PathFigure trajectoryPathFigure;
        private PathFigureCollection trajectoryPathFigureCollection;
        private PathGeometry trajectoryPathGeometry;
        private FormattedText trajectoryText;
        
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

        private int frameIteration = 0;
        private double deltaDistance = 0;
        public double velocity  = 0;
        private float deltaX = 0;
        public string Direction = "N";

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

        // Called when skeleton is not being tracked
        public void Reset()
        {
            this.currentSkeleton = null;
            this.pointList = new List<Point>();
            this.trajectoryPathSegments = null;
            this.trajectoryPathFigure = null;
            this.trajectoryPathFigureCollection = null;
            this.trajectoryPathGeometry = null;
            this.Distance = 0.0;
            this.velocity = 0.0;
            this.Direction = "NA";

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
                    //Assumes that people are walking on a horizontal axis.  deltaX is used to determine the walking direction.
                    deltaX = thisPoint.X - lastPoint.X;
                    Direction = deltaX > 0 ? "R" : "L";

                    //Euclidean distance between the last sampled point and the current point
                    deltaDistance = Math.Sqrt(Math.Pow(((double)thisPoint.X - (double)lastPoint.X), 2.0)
                                                        + Math.Pow(((double)thisPoint.Y - (double)lastPoint.Y), 2.0)
                                                        + Math.Pow(((double)thisPoint.Z - (double)lastPoint.Z), 2.0));

                    //velocity[k] = (distance[k]-distance[k-1])*(framerate/subsampling)
                    velocity = deltaDistance * (30 / FrameSub);

                    //filters out erroneous data
                    if (deltaDistance > MinDeltaDistance && velocity < MaxVelocity)
                    {
                        Distance += deltaDistance;

                        //load data first
                        if (Globals.ds == null)
                        {
                            Globals.ds = new TrajectoryDbDataSet();
                        }

                        //Adds a point to the Point dataset
                        addPointData(thisPoint, Distance, deltaDistance, velocity, Direction, t_key);

                    }

                }

                //stores the point for the next calculation
                lastPoint = thisPoint;
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
                IncrementDistance(currentSkeleton.Position);
            }

            this.InvalidateVisual();
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
        public void addPointData(SkeletonPoint point, double distance, double deltaDistance, double velocity, string direction, TrajectoryDbDataSet.trajectoriesRow t_key)
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
                this.t_key = Globals.ds.trajectories.AddtrajectoriesRow((byte)trackedSkeleton, DateTime.Now, DateTime.Now, 0, "N", 0);

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

        //Called everytime the Trajectory instance is rendered (once per frame). Will draw the trajectory, trajectory info box, skeleton center point
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

                    trajectoryText = new FormattedText("Skeleton " + trackedSkeleton + "\nvelocity: " + this.velocity.ToString("#.##" + " m/s")  + "\nDirection: " + this.Direction
                        + "\nDistance: " + this.Distance,
                        // "\nX: " + currentSkeleton.Position.X + "  Y: " + currentSkeleton.Position.Y + "   Z: " + currentSkeleton.Position.Z,
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
