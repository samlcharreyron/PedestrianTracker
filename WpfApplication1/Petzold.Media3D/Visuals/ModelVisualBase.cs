


using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public abstract class ModelVisualBase : ModelVisual3D
    {
        Point3DCollection verticesPreTransform = new Point3DCollection();
        Vector3DCollection normalsPreTransform = new Vector3DCollection();
        Point3DCollection normalsAsPoints = new Point3DCollection();
        WireFrame wireframe;

        // TODO: Let's have a WireFrame property so color and thickness can be defined
        // Keep the IsWireFrame property as well

        public ModelVisualBase()
        {
            Geometry = Geometry.Clone();
            GeometryModel3D model = new GeometryModel3D(Geometry, null);
            Content = model;

            AlgorithmicTransforms = new AlgorithmicTransformCollection();
        }

        /// <summary>
        ///     Identifies the Name dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string),
            typeof(ModelVisualBase));

        /// <summary>
        ///     Gets or sets the identifying name of the object. ETC.
        ///     This is a dependency property.
        /// </summary>
        public string Name
        {
            set { SetValue(NameProperty, value); }
            get { return (string)GetValue(NameProperty); }
        }

        public static readonly DependencyProperty MaterialProperty =
            GeometryModel3D.MaterialProperty.AddOwner(
                typeof(ModelVisualBase),
                new PropertyMetadata(MaterialPropertyChanged));

        public Material Material
        {
            set { SetValue(MaterialProperty, value); }
            get { return (Material)GetValue(MaterialProperty); }
        }

        public static readonly DependencyProperty BackMaterialProperty =
            GeometryModel3D.BackMaterialProperty.AddOwner(
                typeof(ModelVisualBase),
                new PropertyMetadata(MaterialPropertyChanged));

        public Material BackMaterial
        {
            set { SetValue(BackMaterialProperty, value); }
            get { return (Material)GetValue(BackMaterialProperty); }
        }

        static void MaterialPropertyChanged(DependencyObject obj,
                                            DependencyPropertyChangedEventArgs args)
        {
            (obj as ModelVisualBase).MaterialPropertyChanged(args);
        }

        void MaterialPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            GeometryModel3D model = Content as GeometryModel3D;

            if (args.Property == MaterialProperty)
                model.Material = args.NewValue as Material;

            else if (args.Property == BackMaterialProperty)
                model.BackMaterial = args.NewValue as Material;
        }


        static readonly DependencyPropertyKey GeometryKey =
            DependencyProperty.RegisterReadOnly("Geometry",
                typeof(MeshGeometry3D),
                typeof(ModelVisualBase),
                new PropertyMetadata(new MeshGeometry3D()));

        public static readonly DependencyProperty GeometryProperty =
            GeometryKey.DependencyProperty;

        public MeshGeometry3D Geometry
        {
            protected set { SetValue(GeometryKey, value); }
            get { return (MeshGeometry3D)GetValue(GeometryProperty); }
        }

        public static readonly DependencyProperty AlgorithmicTransformsProperty = 
            DependencyProperty.Register("AlgorithmicTransform",
                typeof(AlgorithmicTransformCollection),
                typeof(ModelVisualBase),
                new PropertyMetadata(null, PropertyChanged));

        public AlgorithmicTransformCollection AlgorithmicTransforms
        {
            set { SetValue(AlgorithmicTransformsProperty, value); }
            get { return (AlgorithmicTransformCollection)GetValue(AlgorithmicTransformsProperty); }
        }

        public static readonly DependencyProperty IsWireFrameProperty =
            DependencyProperty.Register("IsWireFrame",
                typeof(bool),
                typeof(ModelVisualBase),
                new PropertyMetadata(false, PropertyChanged));

        public bool IsWireFrame
        {
            set { SetValue(IsWireFrameProperty, value); }
            get { return (bool)GetValue(IsWireFrameProperty); }
        }

        protected static void PropertyChanged(DependencyObject obj,
                                              DependencyPropertyChangedEventArgs args)
        {
            (obj as ModelVisualBase).PropertyChanged(args);
        }

        protected virtual void PropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            Point3DCollection vertices;
            Vector3DCollection normals;
            Int32Collection indices;
            PointCollection textures;

            if (!IsWireFrame || args.Property == IsWireFrameProperty && !(bool)args.OldValue)
            {
                // Obtain the MeshGeometry3D.
                MeshGeometry3D mesh = Geometry;

                // Get all four collectiona.
                vertices = mesh.Positions;
                normals = mesh.Normals;
                indices = mesh.TriangleIndices;
                textures = mesh.TextureCoordinates;

                // Set the MeshGeometry3D collections to null while updating.
                mesh.Positions = null;
                mesh.Normals = null;
                mesh.TriangleIndices = null;
                mesh.TextureCoordinates = null;
            }

            else
            {
                // Get properties from WireFrame object
                vertices = wireframe.Positions;
                normals = wireframe.Normals;
                indices = wireframe.TriangleIndices;
                textures = wireframe.TextureCoordinates;

                wireframe.Positions = null;
                wireframe.Normals = null;
                wireframe.TriangleIndices = null;
                wireframe.TextureCoordinates = null;
            }

            // If args.Property not IsWireFrame OR Algorithmic transforms
            if (args.Property != AlgorithmicTransformsProperty &&
                args.Property != IsWireFrameProperty)
            {
                // Call the abstract method to fill the collections.
                Triangulate(args, vertices, normals, indices, textures);

                // Transfer vertices and normals to internal collections.
                verticesPreTransform.Clear();
                normalsPreTransform.Clear();

                foreach (Point3D vertex in vertices)
                    verticesPreTransform.Add(vertex);

                foreach (Vector3D normal in normals)
                    normalsPreTransform.Add(normal);
            }

            if (args.Property == AlgorithmicTransformsProperty)
            {
                vertices.Clear();
                normals.Clear();
                normalsAsPoints.Clear();

                // Transfer saved vertices and normals.
                foreach (Point3D vertex in verticesPreTransform)
                    vertices.Add(vertex);

                foreach (Vector3D normal in normalsPreTransform)
                    normalsAsPoints.Add((Point3D)normal);
            }

            if (args.Property != IsWireFrameProperty)
            {
                foreach (AlgorithmicTransform xform in AlgorithmicTransforms)
                {
                    xform.Transform(vertices);
                    xform.Transform(normalsAsPoints);
                }

                foreach (Point3D point in normalsAsPoints)
                    normals.Add((Vector3D)point);
            }

            if (IsWireFrame)
            {
                // Set stuff to WireFrame object, and create it if necessary
                if (wireframe == null)
                {
                    wireframe = new WireFrame();
                    Children.Add(wireframe);                // do we want to remove it when it's no longer used?
                }

                wireframe.TextureCoordinates = textures;
                wireframe.TriangleIndices = indices;
                wireframe.Normals = normals;
                wireframe.Positions = vertices;
            }
            else
            {
                // Obtain the MeshGeometry3D.
                MeshGeometry3D mesh = Geometry;

                // Set the updated collections to the MeshGeometry3D.
                mesh.TextureCoordinates = textures;
                mesh.TriangleIndices = indices;
                mesh.Normals = normals;
                mesh.Positions = vertices;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="textures"></param>
        protected abstract void Triangulate(DependencyPropertyChangedEventArgs args,
                                            Point3DCollection vertices,
                                            Vector3DCollection normals,
                                            Int32Collection indices,
                                            PointCollection textures);

    }
}
