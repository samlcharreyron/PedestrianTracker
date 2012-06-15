//----------------------------------------
// BoxMesh.cs (c) 2007 by Charles Petzold
//----------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a box centered on the origin.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a BoxMesh
    ///     object with multiple 3D visuals. In XAML files, the BoxMesh
    ///     tag will probably appear in a resource section.
    /// </remarks>
    public class BoxMesh : MeshGeneratorBase
    {
        /// <summary>
        ///     Initializes a new instance of the BoxMesh class.
        /// </summary>
        public BoxMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the Width dependency property.
        /// </summary>
        /// <value>
        ///     The width of the box in world units.
        ///     The default is 1. 
        /// </value>
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width",
                typeof(double),
                typeof(BoxMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the width of the box.
        /// </summary>
        public double Width
        {
            set { SetValue(WidthProperty, value); }
            get { return (double)GetValue(WidthProperty); }
        }

        /// <summary>
        ///     Identifies the Height dependency property.
        /// </summary>
        /// <value>
        ///     The height of the box in world units.
        ///     The default is 1. 
        /// </value>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height",
                typeof(double),
                typeof(BoxMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the height of the box.
        /// </summary>
        public double Height
        {
            set { SetValue(HeightProperty, value); }
            get { return (double)GetValue(HeightProperty); }
        }

        /// <summary>
        ///     Identifies the Depth dependency property.
        /// </summary>
        /// <value>
        ///     The depth of the box in world units.
        ///     The default is 1. 
        /// </value>
        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register("Depth",
                typeof(double),
                typeof(BoxMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the depth of the box.
        /// </summary>
        public double Depth
        {
            set { SetValue(DepthProperty, value); }
            get { return (double)GetValue(DepthProperty); }
        }

        /// <summary>
        ///     Identifies the Slices dependency property.
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices",
                typeof(int),
                typeof(BoxMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateDivisions);

        /// <summary>
        ///     Gets or sets the number of divisions across the box width.
        /// </summary>
        /// <value>
        ///     The number of divisions across the box width. 
        ///     This property must be at least 1. 
        ///     The default value is 1.
        /// </value>
        public int Slices
        {
            set { SetValue(SlicesProperty, value); }
            get { return (int)GetValue(SlicesProperty); }
        }

        /// <summary>
        ///     Identifies the Stacks dependency property.
        /// </summary>
        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks",
                typeof(int),
                typeof(BoxMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateDivisions);

        /// <summary>
        ///     Gets or sets the number of divisions in the box height.
        /// </summary>
        /// <value>
        ///     This property must be at least 1. 
        ///     The default value is 1.
        /// </value>
        public int Stacks
        {
            set { SetValue(StacksProperty, value); }
            get { return (int)GetValue(StacksProperty); }
        }

        /// <summary>
        ///     Identifies the Layers dependency property.
        /// </summary>
        public static readonly DependencyProperty LayersProperty =
            DependencyProperty.Register("Layers",
                typeof(int),
                typeof(BoxMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateDivisions);

        /// <summary>
        ///     Gets or sets the number of divisions in the box depth.
        /// </summary>
        /// <value>
        ///     This property must be at least 1. 
        ///     The default value is 1.
        /// </value>
        public int Layers
        {
            set { SetValue(LayersProperty, value); }
            get { return (int)GetValue(LayersProperty); }
        }

        // Validation callback for Slices, Stacks, Layers.
        static bool ValidateDivisions(object obj)
        {
            return (int)obj > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="textures"></param>
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

            double x, y, z;
            int indexBase = 0;

            // Front side.
            // -----------
            z = Depth / 2;

            // Fill the vertices, normals, textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                y = Height / 2 - stack * Height / Stacks;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    x = -Width / 2 + slice * Width / Slices;
                    Point3D point = new Point3D(x, y, z);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(x, y, 0));
                    textures.Add(new Point((double)slice / Slices,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    indices.Add((stack + 0) * (Slices + 1) + slice);
                    indices.Add((stack + 1) * (Slices + 1) + slice);
                    indices.Add((stack + 0) * (Slices + 1) + slice + 1);
 
                    indices.Add((stack + 0) * (Slices + 1) + slice + 1);
                    indices.Add((stack + 1) * (Slices + 1) + slice);
                    indices.Add((stack + 1) * (Slices + 1) + slice + 1);
                }
            }

            // Rear side.
            // -----------
            indexBase = vertices.Count;
            z = -Depth / 2;

            // Fill the vertices, normals, textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                y = Height / 2 - stack * Height / Stacks;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    x = Width / 2 - slice * Width / Slices;
                    Point3D point = new Point3D(x, y, z);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(x, y, 0));
                    textures.Add(new Point((double)slice / Slices,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    indices.Add(indexBase + (stack + 0) * (Slices + 1) + slice);
                    indices.Add(indexBase + (stack + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (stack + 0) * (Slices + 1) + slice + 1);

                    indices.Add(indexBase + (stack + 0) * (Slices + 1) + slice + 1);
                    indices.Add(indexBase + (stack + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (stack + 1) * (Slices + 1) + slice + 1);
                }
            }

            // Left side.
            // -----------
            indexBase = vertices.Count;
            x = -Width / 2;

            // Fill the vertices, normals, textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                y = Height / 2 - stack * Height / Stacks;

                for (int layer = 0; layer <= Layers; layer++)
                {
                    z = -Depth / 2 + layer * Depth / Layers;
                    Point3D point = new Point3D(x, y, z);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(0, y, z));
                    textures.Add(new Point((double)layer / Layers,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int layer = 0; layer < Layers; layer++)
                {
                    indices.Add(indexBase + (stack + 0) * (Layers + 1) + layer);
                    indices.Add(indexBase + (stack + 1) * (Layers + 1) + layer);
                    indices.Add(indexBase + (stack + 0) * (Layers + 1) + layer + 1);

                    indices.Add(indexBase + (stack + 0) * (Layers + 1) + layer + 1);
                    indices.Add(indexBase + (stack + 1) * (Layers + 1) + layer);
                    indices.Add(indexBase + (stack + 1) * (Layers + 1) + layer + 1);
                }
            }

            // Right side.
            // -----------
            indexBase = vertices.Count;
            x = Width / 2;

            // Fill the vertices, normals, textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                y = Height / 2 - stack * Height / Stacks;

                for (int layer = 0; layer <= Layers; layer++)
                {
                    z = Depth / 2 - layer * Depth / Layers;
                    Point3D point = new Point3D(x, y, z);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(0, y, z));
                    textures.Add(new Point((double)layer / Layers,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int layer = 0; layer < Layers; layer++)
                {
                    indices.Add(indexBase + (stack + 0) * (Layers + 1) + layer);
                    indices.Add(indexBase + (stack + 1) * (Layers + 1) + layer);
                    indices.Add(indexBase + (stack + 0) * (Layers + 1) + layer + 1);

                    indices.Add(indexBase + (stack + 0) * (Layers + 1) + layer + 1);
                    indices.Add(indexBase + (stack + 1) * (Layers + 1) + layer);
                    indices.Add(indexBase + (stack + 1) * (Layers + 1) + layer + 1);
                }
            }

            // Top side.
            // -----------
            indexBase = vertices.Count;
            y = Height / 2;

            // Fill the vertices, normals, textures collections.
            for (int layer = 0; layer <= Layers; layer++)
            {
                z = -Depth / 2 + layer * Depth / Layers;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    x = -Width / 2 + slice * Width / Slices;
                    Point3D point = new Point3D(x, y, z);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(x, 0, z));
                    textures.Add(new Point((double)slice / Slices,
                                           (double)layer / Layers));
                }
            }

            // Fill the indices collection.
            for (int layer = 0; layer < Layers; layer++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    indices.Add(indexBase + (layer + 0) * (Slices + 1) + slice);
                    indices.Add(indexBase + (layer + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (layer + 0) * (Slices + 1) + slice + 1);

                    indices.Add(indexBase + (layer + 0) * (Slices + 1) + slice + 1);
                    indices.Add(indexBase + (layer + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (layer + 1) * (Slices + 1) + slice + 1);
                }
            }

            // Bottom side.
            // -----------
            indexBase = vertices.Count;
            y = -Height / 2;

            // Fill the vertices, normals, textures collections.
            for (int layer = 0; layer <= Layers; layer++)
            {
                z = Depth / 2 - layer * Depth / Layers;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    x = -Width / 2 + slice * Width / Slices;
                    Point3D point = new Point3D(x, y, z);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(x, 0, z));
                    textures.Add(new Point((double)slice / Slices,
                                           (double)layer / Layers));
                }
            }

            // Fill the indices collection.
            for (int layer = 0; layer < Layers; layer++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    indices.Add(indexBase + (layer + 0) * (Slices + 1) + slice);
                    indices.Add(indexBase + (layer + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (layer + 0) * (Slices + 1) + slice + 1);

                    indices.Add(indexBase + (layer + 0) * (Slices + 1) + slice + 1);
                    indices.Add(indexBase + (layer + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (layer + 1) * (Slices + 1) + slice + 1);
                }
            }
        }

        /// <summary>
        ///     Creates a new instance of the BoxMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of BoxMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new BoxMesh();
        }
    }
}

