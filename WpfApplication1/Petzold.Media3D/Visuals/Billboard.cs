//------------------------------------------
// Billboard.cs (c) 2007 by Charles Petzold
//------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class Billboard : ModelVisualBase
    {
        public Billboard()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        // Dependency properties

        public static readonly DependencyProperty UpperLeftProperty =
            DependencyProperty.Register("UpperLeft", typeof(Point3D), typeof(Billboard),
                new PropertyMetadata(new Point3D(-1, 1, 0), PropertyChanged));

        public Point3D UpperLeft
        {
            get { return (Point3D)GetValue(UpperLeftProperty); }
            set { SetValue(UpperLeftProperty, value); }
        }

        public static readonly DependencyProperty UpperRightProperty =
            DependencyProperty.Register("UpperRight", typeof(Point3D), typeof(Billboard),
                new PropertyMetadata(new Point3D(1, 1, 0), PropertyChanged));

        public Point3D UpperRight
        {
            get { return (Point3D)GetValue(UpperRightProperty); }
            set { SetValue(UpperRightProperty, value); }
        }

        public static readonly DependencyProperty LowerLeftProperty =
            DependencyProperty.Register("LowerLeft", typeof(Point3D), typeof(Billboard),
                new PropertyMetadata(new Point3D(-1, -1, 0), PropertyChanged));

        public Point3D LowerLeft
        {
            get { return (Point3D)GetValue(LowerLeftProperty); }
            set { SetValue(LowerLeftProperty, value); }
        }

        public static readonly DependencyProperty LowerRightProperty =
            DependencyProperty.Register("LowerRight", typeof(Point3D), typeof(Billboard),
                new PropertyMetadata(new Point3D(1, -1, 0), PropertyChanged));

        public Point3D LowerRight
        {
            get { return (Point3D)GetValue(LowerRightProperty); }
            set { SetValue(LowerRightProperty, value); }
        }

        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices", typeof(int), typeof(Billboard),
                new PropertyMetadata(1, PropertyChanged),
                    ValidateSlicesAndStacks);

        public int Slices
        {
            get { return (int)GetValue(SlicesProperty); }
            set { SetValue(SlicesProperty, value); }
        }

        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks", typeof(int), typeof(Billboard),
                new PropertyMetadata(1, PropertyChanged),
                    ValidateSlicesAndStacks);

        public int Stacks
        {
            get { return (int)GetValue(StacksProperty); }
            set { SetValue(StacksProperty, value); }
        }

        // Private validate method
        static bool ValidateSlicesAndStacks(object value)
        {
            return (int)value > 0;
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

            // Variables for vertices collection.
            Vector3D UL = (Vector3D)UpperLeft;
            Vector3D UR = (Vector3D)UpperRight;
            Vector3D LL = (Vector3D)LowerLeft;
            Vector3D LR = (Vector3D)LowerRight;
            int product = Slices * Stacks;

            // Variables for textures collection
            Point ptOrigin = new Point(0, 0);
            Vector vectSlice = (new Point(1, 0) - ptOrigin) / Slices;
            Vector vectStack = (new Point(0, 1) - ptOrigin) / Stacks;

            for (int stack = 0; stack <= Stacks; stack++)
            {
                for (int slice = 0; slice <= Slices; slice++)
                {
                    vertices.Add((Point3D)(((Stacks - stack) * (Slices - slice) * UL +
                                                      stack  * (Slices - slice) * LL +
                                            (Stacks - stack) * slice * UR +
                                                      stack  * slice * LR) / product));

                    textures.Add(ptOrigin + stack * vectStack + slice * vectSlice);

                    if (slice < Slices && stack < Stacks)
                    {
                        indices.Add((Slices + 1) * stack + slice);
                        indices.Add((Slices + 1) * (stack + 1) + slice);
                        indices.Add((Slices + 1) * stack + slice + 1);

                        indices.Add((Slices + 1) * stack + slice + 1);
                        indices.Add((Slices + 1) * (stack + 1) + slice);
                        indices.Add((Slices + 1) * (stack + 1) + slice + 1);
                    }
                }
            }
        }
    }
}
