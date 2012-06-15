//
//
//
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class Cuboid : ModelVisualBase
    {
        public Cuboid()
        {
            PropertyChanged(this, new DependencyPropertyChangedEventArgs());
        }
        // Width property.
        // ---------------
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width",
                typeof(double), typeof(Cuboid),
                new PropertyMetadata(1.0, PropertyChanged));

        public double Width
        {
            set { SetValue(WidthProperty, value); }
            get { return (double)GetValue(WidthProperty); }
        }

        // Height property.
        // ----------------
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height",
                typeof(double), typeof(Cuboid),
                new PropertyMetadata(1.0, PropertyChanged));

        public double Height
        {
            set { SetValue(HeightProperty, value); }
            get { return (double)GetValue(HeightProperty); }
        }

        // Depth property.
        // ---------------
        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register("Depth",
                typeof(double), typeof(Cuboid),
                new PropertyMetadata(1.0, PropertyChanged));

        public double Depth
        {
            set { SetValue(DepthProperty, value); }
            get { return (double)GetValue(DepthProperty); }
        }

        // Origin property.
        // ----------------

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin",
                typeof(Point3D), typeof(Cuboid),
                new PropertyMetadata(new Point3D(-0.5, -0.5, -0.5),
                                     PropertyChanged));

        public Point3D Origin
        {
            set { SetValue(OriginProperty, value); }
            get { return (Point3D)GetValue(OriginProperty); }
        }


        // Slices property.
        // ----------------
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices", typeof(int), typeof(Cuboid),
                new PropertyMetadata(10, PropertyChanged),
                    ValidateSlices);

        public int Slices
        {
            get { return (int)GetValue(SlicesProperty); }
            set { SetValue(SlicesProperty, value); }
        }

        // Stacks property.
        // ----------------
        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks", typeof(int), typeof(Cuboid),
                new PropertyMetadata(10, PropertyChanged),
                    ValidateSlices);

        public int Stacks
        {
            get { return (int)GetValue(StacksProperty); }
            set { SetValue(StacksProperty, value); }
        }

        // Slivers property.
        // ----------------
        public static readonly DependencyProperty SliversProperty =
            DependencyProperty.Register("Slivers", typeof(int), typeof(Cuboid),
                new PropertyMetadata(10, PropertyChanged),
                    ValidateSlices);

        public int Slivers
        {
            get { return (int)GetValue(SliversProperty); }
            set { SetValue(SliversProperty, value); }
        }

        static bool ValidateSlices(object obj)
        {
            return (int)obj > 0;
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

            // Front.
            for (int iy = 0; iy <= Stacks; iy++)
            {
                double y = Origin.Y + Height - iy * Height / Stacks;

                for (int ix = 0; ix <= Slices; ix++)
                {
                    double x = Origin.X + ix * Width / Slices;
                    vertices.Add(new Point3D(x, y, Origin.Z + Depth));
                }
            }

            // Back
            for (int iy = 0; iy <= Stacks; iy++)
            {
                double y = Origin.Y + Height - iy * Height / Stacks;

                for (int ix = 0; ix <= Slices; ix++)
                {
                    double x = Origin.X + Width - ix * Width / Slices;
                    vertices.Add(new Point3D(x, y, Origin.Z));
                }
            }

            // Left
            for (int iy = 0; iy <= Stacks; iy++)
            {
                double y = Origin.Y + Height - iy * Height / Stacks;

                for (int iz = 0; iz <= Slivers; iz++)
                {
                    double z = Origin.Z + iz * Depth / Slivers;
                    vertices.Add(new Point3D(Origin.X, y, z));
                }
            }

            // Right
            for (int iy = 0; iy <= Stacks; iy++)
            {
                double y = Origin.Y + Height - iy * Height / Stacks;

                for (int iz = 0; iz <= Slivers; iz++)
                {
                    double z = Origin.Z + Depth - iz * Depth / Slivers;
                    vertices.Add(new Point3D(Origin.X + Width, y, z));
                }
            }

            // Top
            for (int iz = 0; iz <= Slivers; iz++)
            {
                double z = Origin.Z + iz * Depth / Slivers;

                for (int ix = 0; ix <= Slices; ix++)
                {
                    double x = Origin.X + ix * Width / Slices;
                    vertices.Add(new Point3D(x, Origin.Y + Height, z));
                }
            }

            // Top
            for (int iz = 0; iz <= Slivers; iz++)
            {
                double z = Origin.Z + Depth - iz * Depth / Slivers;

                for (int ix = 0; ix <= Slices; ix++)
                {
                    double x = Origin.X + ix * Width / Slices;
                    vertices.Add(new Point3D(x, Origin.Y, z));
                }
            }

            for (int side = 0; side < 6; side++)
            {
                for (int iy = 0; iy <= Stacks; iy++)
                {
                    double y = (double)iy / Stacks;

                    for (int ix = 0; ix <= Slices; ix++)
                    {
                        double x = (double)ix / Slices;
                        textures.Add(new Point(x, y));
                    }
                }
            }

            // Front, back, left, right
            for (int side = 0; side < 6; side++)
            {
                for (int iy = 0; iy < Stacks; iy++)
                    for (int ix = 0; ix < Slices; ix++)
                    {
                        indices.Add(side * (Slices + 1) * (Stacks + 1) + iy * (Slices + 1) + ix);
                        indices.Add(side * (Slices + 1) * (Stacks + 1) + (iy + 1) * (Slices + 1) + ix);
                        indices.Add(side * (Slices + 1) * (Stacks + 1) + iy * (Slices + 1) + ix + 1);

                        indices.Add(side * (Slices + 1) * (Stacks + 1) + iy * (Slices + 1) + ix + 1);
                        indices.Add(side * (Slices + 1) * (Stacks + 1) + (iy + 1) * (Slices + 1) + ix);
                        indices.Add(side * (Slices + 1) * (Stacks + 1) + (iy + 1) * (Slices + 1) + ix + 1);
                    }
            }
        }
    }
}
