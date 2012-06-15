//---------------------------------------
// Sphere.cs (c) 2007 by Charles Petzold
//---------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class Sphere : ModelVisualBase
    {
        // Objects reused by calls to Triangulate.
        AxisAngleRotation3D rotate;
        RotateTransform3D xform;

        // Public constructor to initialize those fields, etc
        public Sphere()
        {
            rotate = new AxisAngleRotation3D();
            xform = new RotateTransform3D(rotate);

            PropertyChanged(this, new DependencyPropertyChangedEventArgs());
        }

        // Dependency properties
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices", typeof(int), typeof(Sphere),
                new PropertyMetadata(36, PropertyChanged),
                    ValidatePositive);

        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks", typeof(int), typeof(Sphere),
                new PropertyMetadata(18, PropertyChanged),
                    ValidatePositive);

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point3D), typeof(Sphere),
                new PropertyMetadata(new Point3D(0, 0, 0), PropertyChanged));

        public static readonly DependencyProperty LongitudeFromProperty =
            DependencyProperty.Register("LongitudeFrom", typeof(Double), typeof(Sphere),
                new PropertyMetadata(-180.0, PropertyChanged),
                    ValidateLongitude);

        public static readonly DependencyProperty LongitudeToProperty =
            DependencyProperty.Register("LongitudeTo", typeof(Double), typeof(Sphere),
                new PropertyMetadata(180.0, PropertyChanged),
                    ValidateLongitude);

        public static readonly DependencyProperty LatitudeFromProperty =
            DependencyProperty.Register("LatitudeFrom", typeof(Double), typeof(Sphere),
                new PropertyMetadata(90.0, PropertyChanged),
                    ValidateLatitude);

        public static readonly DependencyProperty LatitudeToProperty =
            DependencyProperty.Register("LatitudeTo", typeof(Double), typeof(Sphere),
                new PropertyMetadata(-90.0, PropertyChanged),
                    ValidateLatitude);

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Sphere),
                new PropertyMetadata(1.0, PropertyChanged),
                    ValidateNonNegative);

        // CLR properties
        public int Slices
        {
            get { return (int)GetValue(SlicesProperty); }
            set { SetValue(SlicesProperty, value); }
        }

        public int Stacks
        {
            get { return (int)GetValue(StacksProperty); }
            set { SetValue(StacksProperty, value); }
        }

        public Point3D Center
        {
            get { return (Point3D)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public Double LongitudeFrom
        {
            get { return (Double)GetValue(LongitudeFromProperty); }
            set { SetValue(LongitudeFromProperty, value); }
        }

        public Double LongitudeTo
        {
            get { return (Double)GetValue(LongitudeToProperty); }
            set { SetValue(LongitudeFromProperty, value); }
        }

        public Double LatitudeFrom
        {
            get { return (Double)GetValue(LatitudeFromProperty); }
            set { SetValue(LatitudeFromProperty, value); }
        }

        public Double LatitudeTo
        {
            get { return (Double)GetValue(LatitudeToProperty); }
            set { SetValue(LatitudeToProperty, value); }
        }

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Private validate methods
        static bool ValidatePositive(object value)
        {
            return (int)value > 0;
        }

        static bool ValidateNonNegative(object value)
        {
            return (double)value >= 0;
        }

        static bool ValidateLongitude(object value)
        {
            Double d = (double)value;
            return d >= -180 && d <= 180;
        }

        static bool ValidateLatitude(object value)
        {
            Double d = (double)value;
            return d >= -90 && d <= 90;
        }

        protected override void Triangulate(DependencyPropertyChangedEventArgs args, 
                                            Point3DCollection vertices, 
                                            Vector3DCollection normals, 
                                            Int32Collection indices, 
                                            PointCollection textures)
        {
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            textures.Clear();

            // Copy properties to local variables to improve speed
            int slices = Slices;
            int stacks = Stacks;
            double radius = Radius;
            Point3D ctr = Center;

            double lat1 = Math.Max(LatitudeFrom, LatitudeTo);   // default is 90
            double lat2 = Math.Min(LatitudeFrom, LatitudeTo);   // default is -90

            double lng1 = LongitudeFrom;            // default is -180
            double lng2 = LongitudeTo;              // default is 180

            for (int lat = 0; lat <= stacks; lat++)
            {
                double degrees = lat1 - lat * (lat1 - lat2) / stacks;

                double angle = Math.PI * degrees / 180;
                double y = radius * Math.Sin(angle);
                double scale = Math.Cos(angle);

                for (int lng = 0; lng <= slices; lng++)
                {
                    double diff = lng2 - lng1;

                    if (diff < 0)
                        diff += 360;

                    degrees = lng1 + lng * diff / slices;
                    angle = Math.PI * degrees / 180;
                    double x = radius * scale * Math.Sin(angle);
                    double z = radius * scale * Math.Cos(angle);

                    Vector3D vect = new Vector3D(x, y, z);
                    vertices.Add(ctr + vect);
                    normals.Add(vect);
                    textures.Add(new Point((double)lng / slices,
                                           (double)lat / stacks));
                }
            }

            for (int lat = 0; lat < stacks; lat++)
            {
                int start = lat * (slices + 1);
                int next = start + slices + 1;

                for (int lng = 0; lng < slices; lng++)
                {
                    indices.Add(start + lng);
                    indices.Add(next + lng);
                    indices.Add(next + lng + 1);

                    indices.Add(start + lng);
                    indices.Add(next + lng + 1);
                    indices.Add(start + lng + 1);
                }
            }
        }
    }
}
