//--------------------------------------------
// ModelVisual.cs (c) 2007 by Charles Petzold
//--------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class ModelVisual : ModelVisualBase
    {
        public static readonly DependencyProperty MeshGeneratorProperty =
            DependencyProperty.Register("MeshGeometry",
                typeof(MeshGeneratorBase),
                typeof(ModelVisual),
                new PropertyMetadata(null, PropertyChanged));

        public MeshGeneratorBase MeshGenerator
        {
            set { SetValue(MeshGeneratorProperty, value); }
            get { return (MeshGeneratorBase)GetValue(MeshGeneratorProperty); }
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

            MeshGeometry3D mesh = MeshGenerator.Geometry;

            foreach (Point3D vertex in mesh.Positions)
                vertices.Add(vertex);

            foreach (Vector3D normal in mesh.Normals)
                normals.Add(normal);

            foreach (int index in mesh.TriangleIndices)
                indices.Add(index);

            foreach (Point texture in mesh.TextureCoordinates)
                textures.Add(texture);
        }
    }
}
