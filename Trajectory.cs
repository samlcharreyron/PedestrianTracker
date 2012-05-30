using System;
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
using Microsoft.Kinect;

public class Trajectory
{
    private PathFigure trajectoryPathFigure;
    private Skeleton s;
    private DrawingContext dc;
    private List<Point> pointList;

	public Trajectory(Skeleton s, DrawingContext dc)
	{
        
        this.s = s;
        this.dc = dc;
        this.pointList = new List<Point>();

        this.trajectoryPathFigure = new PathFigure();
        this.tr
                trajectoryPathFigure.StartPoint = SkeletonPointToScreen(s.Position);
                trajectoryPathFigure.IsClosed = false;
                trajectoryPathGeometry.Figures.Add(trajectoryPathFigure);
	}
}
