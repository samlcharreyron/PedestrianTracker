//-----------------------------------------
// TubeMesh.cs (c) 2007 by Charles Petzold
//-----------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a tube.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a 
    ///     CylinderMesh object with multiple 3D visuals. 
    ///     In XAML files, the TubeMesh
    ///     tag will probably appear in a resource section.
    ///     The cylinder is centered on the positive Y axis.
    /// </remarks>
    public class TubeMesh : CylindricalMeshBase
    {
        /// <summary>
        ///     Initializes a new instance of the TubeMesh class.
        /// </summary>
        public TubeMesh()
        {
            // Initialize Geometry property.
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the Thickness dependency property.
        /// </summary>
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness",
                typeof(double),
                typeof(TubeMesh),
                new PropertyMetadata(0.25, PropertyChanged),
                ValidateThickness);

        // Validation callback for Thickness.
        static bool ValidateThickness(object obj)
        {
            return (double)obj >= 0;
        }

        /// <summary>
        ///     Gets or sets the thickness of the wall of the tube.
        /// </summary>
        public double Thickness
        {
            set { SetValue(ThicknessProperty, value); }
            get { return (double)GetValue(ThicknessProperty); }
        }

        /// <summary>
        ///     Identifies the EndStacks dependency property.
        /// </summary>
        public static readonly DependencyProperty EndStacksProperty =
            DependencyProperty.Register("EndStacks",
                typeof(int),
                typeof(TubeMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateEndStacks);

        // Validation callback for EndStacks.
        static bool ValidateEndStacks(object obj)
        {
            return (int)obj > 0;
        }

        /// <summary>
        ///     Gets or sets the number of radial divisions of the wall of 
        ///     the tube.
        /// </summary>
        /// <value>
        ///     The number of radial divisions on the wall of the tube. 
        ///     This property must be at least 1, which is also the default value. 
        /// </value>
        /// <remarks>
        ///     The default value of 1 is appropriate in many cases. 
        ///     However, if PointLight or SpotLight objects are applied to the
        ///     tube, or if non-linear transforms are used to deform
        ///     the figure, you should set EndStacks to a higher value.
        /// </remarks>
        public int EndStacks
        {
            set { SetValue(EndStacksProperty, value); }
            get { return (int)GetValue(EndStacksProperty); }
        }

        /// <summary>
        ///     Identifies the Fold dependency property.
        /// </summary>
        public static readonly DependencyProperty FoldProperty =
            DependencyProperty.Register("Fold",
                typeof(double),
                typeof(TubeMesh),
                new PropertyMetadata(0.125, PropertyChanged),
                ValidateFold);

        // Validation callback for Fold.
        static bool ValidateFold(object obj)
        {
            return (double)obj < 0.5;
        }

        /// <summary>
        ///     Gets or sets the fraction of the brush that appears on
        ///     the top and bottom ends of the tube.
        /// </summary>
        /// <value>
        ///     The fraction of the brush that folds over the top and
        ///     bottom ends of the tube. The default is 0.1. The
        ///     property cannot be greater than 0.5.
        /// </value>
        public double Fold
        {
            set { SetValue(FoldProperty, value); }
            get { return (double)GetValue(FoldProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///     The DependencyPropertyChangedEventArgs object originally 
        ///     passed to the PropertyChanged handler that initiated this
        ///     recalculation.
        /// </param>
        /// <param name="vertices">
        ///     The Point3DCollection corresponding to the Positions property
        ///     of the MeshGeometry3D.
        /// </param>
        /// <param name="normals">
        ///     The Vector3DCollection corresponding to the Normals property
        ///     of the MeshGeometry3D.
        /// </param>
        /// <param name="indices">
        ///     The Int32Collection corresponding to the TriangleIndices
        ///     property of the MeshGeometry3D.
        /// </param>
        /// <param name="textures">
        ///     The PointCollection corresponding to the TextureCoordinates
        ///     property of the MeshGeometry3D.
        /// </param>
        protected override void Triangulate(DependencyPropertyChangedEventArgs args,
                                            Point3DCollection vertices,
                                            Vector3DCollection normals,
                                            Int32Collection indices,
                                            PointCollection textures)
        {
            // Clear all four collections.
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            textures.Clear();

            // Loop for outside (side = 1) and inside (side = -1).
            for (int side = 1; side >= -1; side -= 2)
            {
                int offset = vertices.Count;

                // Begin at the top end. Fill the collections.
                for (int stack = 0; stack <= EndStacks; stack++)
                {
                    double y = Length;
                    double radius = Radius + side * stack * Thickness / 2 / EndStacks;
                    int top = offset + (stack + 0) * (Slices + 1);
                    int bot = offset + (stack + 1) * (Slices + 1);

                    for (int slice = 0; slice <= Slices; slice++)
                    {
                        double theta = slice * 2 * Math.PI / Slices;
                        double x = -radius * Math.Sin(theta);
                        double z = -radius * Math.Cos(theta);

                        vertices.Add(new Point3D(x, y, z));
                        normals.Add(new Vector3D(0, side, 0));
                        textures.Add(new Point((double)slice / Slices,
                                               Fold * stack / EndStacks));

                        if (stack < EndStacks && slice < Slices)
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

                // Length of the tube: Fill in the collections.
                for (int stack = 0; stack <= Stacks; stack++)
                {
                    double y = Length - stack * Length / Stacks;
                    int top = offset + (stack + 0) * (Slices + 1);
                    int bot = offset + (stack + 1) * (Slices + 1);

                    for (int slice = 0; slice <= Slices; slice++)
                    {
                        double theta = slice * 2 * Math.PI / Slices;
                        double x = -(Radius + side * Thickness / 2) * Math.Sin(theta);
                        double z = -(Radius + side * Thickness / 2) * Math.Cos(theta);

                        vertices.Add(new Point3D(x, y, z));
                        normals.Add(new Vector3D(side * x, 0, side * z));
                        textures.Add(new Point((double)slice / Slices,
                                               Fold + (1 - 2 * Fold) * stack / Stacks));

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

                // Finish with the bottom end. Fill the collections.
                for (int stack = 0; stack <= EndStacks; stack++)
                {
                    double y = 0;
                    double radius = Radius + side * Thickness / 2 * (1 - (double)stack / EndStacks);
                    int top = offset + (stack + 0) * (Slices + 1);
                    int bot = offset + (stack + 1) * (Slices + 1);

                    for (int slice = 0; slice <= Slices; slice++)
                    {
                        double theta = slice * 2 * Math.PI / Slices;
                        double x = -radius * Math.Sin(theta);
                        double z = -radius * Math.Cos(theta);

                        vertices.Add(new Point3D(x, y, z));
                        normals.Add(new Vector3D(0, -side, 0));
                        textures.Add(new Point((double)slice / Slices,
                                               (1 - Fold) + Fold * stack / EndStacks));

                        if (stack < EndStacks && slice < Slices)
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
            }
        }

        /// <summary>
        ///     Creates a new instance of the TubeMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of TubeMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new TubeMesh();
        }
    }
}

