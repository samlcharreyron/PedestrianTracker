//-----------------------------------------
// Cylinder.cs (c) 2007 by Charles Petzold
//-----------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class Cylinder : ModelVisualBase
    {
        // Objects reused by calls to Triangulate.
        AxisAngleRotation3D rotate;
        RotateTransform3D xform;

        // Public constructor to initialize those fields, etc
        public Cylinder()
        {
            rotate = new AxisAngleRotation3D();
            xform = new RotateTransform3D(rotate);

            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the <c>Point1</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty Point1Property =
            DependencyProperty.Register("Point1", 
                typeof(Point3D), 
                typeof(Cylinder),
                new PropertyMetadata(new Point3D(0, 1, 0), PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Point3D Point1
        {
            set { SetValue(Point1Property, value); }
            get { return (Point3D)GetValue(Point1Property); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty Point2Property =
            DependencyProperty.Register("Point2", 
                typeof(Point3D), 
                typeof(Cylinder),
                new PropertyMetadata(new Point3D(0, 0, 0), PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Point3D Point2
        {
            get { return (Point3D)GetValue(Point2Property); }
            set { SetValue(Point2Property, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty Radius1Property =
            DependencyProperty.Register("Radius1", 
                typeof(double), 
                typeof(Cylinder),
                new PropertyMetadata(1.0, PropertyChanged),
                delegate(object value) { return (double)value >= 0; });

        /// <summary>
        /// 
        /// </summary>
        public double Radius1
        {
            set { SetValue(Radius1Property, value); }
            get { return (double)GetValue(Radius1Property); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty Radius2Property =
            DependencyProperty.Register("Radius2", 
                typeof(double), 
                typeof(Cylinder),
                new PropertyMetadata(1.0, PropertyChanged),
                delegate(object value) { return (double)value >= 0; });

        /// <summary>
        /// 
        /// </summary>
        public double Radius2
        {
            set { SetValue(Radius2Property, value); }
            get { return (double)GetValue(Radius2Property); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty Fold1Property =
            DependencyProperty.Register("Fold1",
                typeof(double),
                typeof(Cylinder),
                new PropertyMetadata(0.1, PropertyChanged),
                delegate(object value) { return (double)value >= 0 && (double)value <= 1; });

        /// <summary>
        /// 
        /// </summary>
        public double Fold1
        {
            get { return (double)GetValue(Fold1Property); }
            set { SetValue(Fold1Property, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty Fold2Property =
            DependencyProperty.Register("Fold2", 
                typeof(double), 
                typeof(Cylinder),
                new PropertyMetadata(0.9, PropertyChanged),
                delegate(object value) { return (double)value >= 0 && (double)value <= 1; });

        /// <summary>
        /// 
        /// </summary>
        public double Fold2
        {
            get { return (double)GetValue(Fold2Property); }
            set { SetValue(Fold2Property, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices", 
                typeof(int), 
                typeof(Cylinder),
                new PropertyMetadata(36, PropertyChanged),
                delegate(object value) { return (int)value > 2; });

        /// <summary>
        /// 
        /// </summary>
        public int Slices
        {
            set { SetValue(SlicesProperty, value); }
            get { return (int)GetValue(SlicesProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks", 
                typeof(int), 
                typeof(Cylinder),
                new PropertyMetadata(1, PropertyChanged),
                delegate(object value) { return (int)value > 0; });

        /// <summary>
        /// 
        /// </summary>
        public int Stacks
        {
            set { SetValue(StacksProperty, value); }
            get { return (int)GetValue(StacksProperty); }
        }

        /// <summary>
        ///     Identifies the EndStacks dependency property.
        /// </summary>
        public static readonly DependencyProperty EndStacksProperty =
            DependencyProperty.Register("EndStacks",
                typeof(int),
                typeof(Cylinder),
                new PropertyMetadata(1, PropertyChanged),
                delegate(object value) { return (int)value > 0; });

        /// <summary>
        ///     Gets or sets the number of radial divisions on each end of 
        ///     the cylinder.
        /// </summary>
        /// <value>
        ///     The number of radial divisions on the end of the cylinder. 
        ///     This property must be at least 1, which is also the default value. 
        /// </value>
        /// <remarks>
        ///     The default value of 1 is appropriate in many cases. 
        ///     However, if PointLight or SpotLight objects are applied to the
        ///     cylinder, or if non-linear transforms are used to deform
        ///     the figure, you should set EndStacks to a higher value.
        /// </remarks>
        public int EndStacks
        {
            set { SetValue(EndStacksProperty, value); }
            get { return (int)GetValue(EndStacksProperty); }
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

            // vectRearRadius points towards -Z (when possible).
            Vector3D vectCylinder = Point2 - Point1;
            Vector3D vectRearRadius;

            if (vectCylinder.X == 0 && vectCylinder.Y == 0)
            {
                // Special case: set rear-radius vector
                vectRearRadius = new Vector3D(0, -1, 0);
            }
            else
            {
                // Find vector axis 90 degrees from cylinder where Z == 0
                rotate.Axis = Vector3D.CrossProduct(vectCylinder, new Vector3D(0, 0, 1));
                rotate.Angle = -90;

                // Rotate cylinder 90 degrees to find radius vector
                vectRearRadius = vectCylinder * xform.Value;
                vectRearRadius.Normalize();
            }

            // Will rotate radius around cylinder axis
            rotate.Axis = -vectCylinder;

            // Begin at the top end. Fill the collections.
            for (int stack = 0; stack <= EndStacks; stack++)
            {
                double radius = stack * Radius1 / EndStacks;
                Vector3D vectRadius = radius * vectRearRadius;
                int top = (stack + 0) * (Slices + 1);
                int bot = (stack + 1) * (Slices + 1);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    rotate.Angle = slice * 360.0 / Slices;
                    vertices.Add(Point1 + vectRadius * xform.Value);
                    normals.Add(-vectCylinder);
                    textures.Add(new Point((double)slice / Slices,
                                           Fold1 * stack / EndStacks));

                    if (stack < EndStacks && slice < Slices)
                    {
                        if (stack != 0)
                        {
                            indices.Add(top + slice);
                            indices.Add(bot + slice);
                            indices.Add(top + slice + 1);
                        }
                        indices.Add(top + slice + 1);
                        indices.Add(bot + slice);
                        indices.Add(bot + slice + 1);
                    }
                }
            }

            int offset = vertices.Count;

            // Go down length of cylinder and fill in the collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                double radius = ((Stacks - stack) * Radius1 + stack * Radius2) / Stacks;
                Vector3D vectRadius = radius * vectRearRadius;
                Point3D center = (Point3D) (Point1 + stack * vectCylinder / Stacks);
                int top = offset + (stack + 0) * (Slices + 1);
                int bot = offset + (stack + 1) * (Slices + 1);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    rotate.Angle = slice * 360.0 / Slices;
                    Vector3D normal = vectRadius * xform.Value;
                    normals.Add(normal);
                    vertices.Add(center + normal);
                    textures.Add(new Point((double)slice / Slices,
                                          Fold1 + (Fold2 - Fold1) * stack / Stacks));

                    if (stack < Stacks && slice < Slices)
                    {
                        indices.Add(top + slice);
                        indices.Add(bot + slice);
                        indices.Add(top + slice + 1);

                        indices.Add(top + slice + 1);
                        indices.Add(bot + slice);
                        indices.Add(bot + slice + 1);
                    }
                }
            }

            offset = vertices.Count;

            // Finish with bottom.
            for (int stack = 0; stack <= EndStacks; stack++)
            {
                double radius = Radius2 * (1 - (double)stack / EndStacks);
                Vector3D vectRadius = radius * vectRearRadius;
                int top = offset + (stack + 0) * (Slices + 1);
                int bot = offset + (stack + 1) * (Slices + 1);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    rotate.Angle = slice * 360.0 / Slices;
                    vertices.Add(Point2 + vectRadius * xform.Value);
                    normals.Add(vectCylinder);
                    textures.Add(new Point((double)slice / Slices,
                                           Fold2 + (1 - Fold2) * stack / EndStacks));

                    if (stack < EndStacks && slice < Slices)
                    {
                        indices.Add(top + slice);
                        indices.Add(bot + slice);
                        indices.Add(top + slice + 1);

                        if (stack != EndStacks - 1)
                        {
                            indices.Add(top + slice + 1);
                            indices.Add(bot + slice);
                            indices.Add(bot + slice + 1);
                        }
                    }
                }
            }
        }
    }
}
