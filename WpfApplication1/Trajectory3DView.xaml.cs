using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using _3DTools;
using Petzold.Media3D;
using PedestrianTracker.Properties;

namespace PedestrianTracker
{
    /// <summary>
    /// Interaction logic for _3DView.xaml
    /// </summary>
    public partial class Trajectory3DView : Window
    {

        
        private GeometryModel3D mGeometry;

        private bool mDown;
        private Point mLastPos;

        public Trajectory3DView()
        {
            InitializeComponent();

            model.Transform = new Transform3DGroup();

            Axes axes = new Axes()
                {
                    Color = Colors.White
                };

            model.Children.Add(axes);

            WireLine roadAxis = new WireLine()
            {
                Point1 = new Point3D(-10, 0, 0),
                Point2 = new Point3D(10, 0, 0),
                Thickness = 5.0,
                Color = Colors.BlueViolet,
                ArrowEnds = Petzold.Media2D.ArrowEnds.End,
                Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), Settings.Default.KinectAngle))
            };

            model.Children.Add(roadAxis);

            if (Globals.ds.trajectories.Count > 0)
            {
                DrawTrajectory(Globals.ds.trajectories[Globals.ds.trajectories.Count - 1].t_id,Colors.Red);
            }

        }

        private void BuildPlane()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(-10,-0.3,10));
            mesh.Positions.Add(new Point3D(-10,-2.4,-50));
            mesh.Positions.Add(new Point3D(10,-1.75,-50));
            mesh.Positions.Add(new Point3D(10,0.3,10));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);

            mGeometry = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.YellowGreen));
            mGeometry.Transform = new Transform3DGroup();
            group.Children.Add(mGeometry);
        }

        private void DrawTrajectory(int t_id,Color color)
        {

            //find trajectory id of last inserted trajectory

            if (Globals.ds.trajectories.Count == 0)
            {
                return;
            }

            //int t_id = Globals.ds.trajectories[Globals.ds.trajectories.Count - 1].t_id;

            TrajectoryDbDataSet.pointsRow[] pointsRows = (TrajectoryDbDataSet.pointsRow[])Globals.ds.points.Select("t_id = " + t_id);
            Point3D firstPoint = new Point3D((float)pointsRows[0].X,(float)pointsRows[0].Y,(float)pointsRows[0].Z);

            Point3DCollection pointCollection = new Point3DCollection();

            for (int i = 1; i < pointsRows.Length; i++ )
            {
                TrajectoryDbDataSet.pointsRow currentRow;
                TrajectoryDbDataSet.pointsRow lastRow;
                
                try
                {
                    currentRow = pointsRows[i];
                    lastRow = pointsRows[i-1];
                }
                catch (Exception e)
                {
                    return;
                }

                pointCollection.Add(new Point3D(currentRow.X, currentRow.Y, currentRow.Z));

            }

             WirePolyline wl = new WirePolyline()
            {
                Points = pointCollection,
                Thickness = 4,
                Rounding = 3,
                Color = color, 
            };

            model.Children.Add(wl);
            model.Transform = new Transform3DGroup();
        }

              
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z - e.Delta / 250D);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            camera.Position = new Point3D(camera.Position.X, camera.Position.Y, 5);
            model.Transform = new Transform3DGroup();
            
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mDown)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;

                double mouseAngle = 0;
                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                double axisAngle = mouseAngle + Math.PI / 2;

                Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

                double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                Transform3DGroup group = model.Transform as Transform3DGroup;
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
                group.Children.Add(new RotateTransform3D(r));

                mLastPos = actualPos;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            mDown = true;
            Point pos = Mouse.GetPosition(viewport);
            mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mDown = false;
        }

        private void ShowAllButton_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (TrajectoryDbDataSet.trajectoriesRow row in Globals.ds.trajectories.Rows)
            {
                if (row.t_id != Globals.ds.trajectories[Globals.ds.trajectories.Count - 1].t_id)
                {
                    //Choose random color for trajectory
                    Random r = new Random();
                    byte red = (byte)r.Next(0, byte.MaxValue + 1);
                    byte green = (byte)r.Next(0, byte.MaxValue + 1);
                    byte blue = (byte)r.Next(0, byte.MaxValue + 1);

                    DrawTrajectory(row.t_id, Color.FromRgb(red, green, blue));
                }
            }
        }
    }
}
