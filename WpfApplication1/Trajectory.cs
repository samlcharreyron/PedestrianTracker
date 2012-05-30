﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private PathSegmentCollection trajectoryPathSegments;
        private PathFigure trajectoryPathFigure;
        private PathFigureCollection trajectoryPathFigureCollection;
        private PathGeometry trajectoryPathGeometry;

        private FormattedText trajectoryText;

        private Brush TrajectoryBrush;
        

        private readonly Brush centerPointBrush = Brushes.Black;
        private Brush TrajectoryTextBrush = Brushes.Gray;

        private const double BodyCenterThickness = 8;

        private const double textOffset = 10;

        private int frameIteration = 0;

        private double deltaDistance = 0;
        public double velocity = 0;
   
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
                    deltaDistance = Math.Sqrt(Math.Pow(((double)thisPoint.X - (double)lastPoint.X), 2.0)
                                                        + Math.Pow(((double)thisPoint.Y - (double)lastPoint.Y), 2.0)
                                                        + Math.Pow(((double)thisPoint.Z - (double)lastPoint.Z), 2.0));

                    if (deltaDistance > 0.01)
                    {
                        Distance += deltaDistance;
                        velocity = deltaDistance * 10;
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

            //if only one point do not calculate the distance
            if (pointList.Count > 1)
            {
                IncrementDistance(lastPoint, currentSkeleton.Position);
            }

            lastPoint = currentSkeleton.Position;

            this.InvalidateVisual();
        }

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

                    trajectoryText = new FormattedText("Skeleton " + trackedSkeleton + "\nvelocity: " + this.velocity.ToString("#.##" + " m/s"),
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
                
            if (trajectoryPathGeometry != null && currentSkeleton != null & currentSkeleton.TrackingState!=SkeletonTrackingState.NotTracked)
            {
                drawingContext.DrawGeometry(null, new Pen(this.TrajectoryBrush,2), this.trajectoryPathGeometry);
            }
        }

    }
}
