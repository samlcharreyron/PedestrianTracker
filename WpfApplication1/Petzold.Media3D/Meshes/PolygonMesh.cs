//--------------------------------------------
// PolygonMesh.cs (c) 2007 by Charles Petzold 
//--------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public class PolygonMesh : FlatSurfaceMeshBase
    {
        /// <summary>
        /// 
        /// </summary>
        public PolygonMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty LengthProperty =
            DependencyProperty.Register("Length",
                typeof(double),
                typeof(PolygonMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public double Length
        {
            set { SetValue(LengthProperty, value); }
            get { return (double)GetValue(LengthProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SidesProperty =                    // check that greater than 2 !!!!!
            DependencyProperty.Register("Sides",
                typeof(int),
                typeof(PolygonMesh),
                new PropertyMetadata(5, PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public int Sides
        {
            set { SetValue(SidesProperty, value); }
            get { return (int)GetValue(SidesProperty); }
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
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            textures.Clear();

            Vector3D normal = new Vector3D(0, 0, 1);
            double angleInner = 2 * Math.PI / Sides;
            double radius = Length / 2 / Math.Sin(angleInner / 2);
            double angle = 3 * Math.PI / 2 + angleInner / 2;
            double xMin = 0, xMax = 0, yMin = 0, yMax = 0;

            for (int side = 0; side < Sides; side++)
            {
                double x = Math.Cos(angle);
                double y = Math.Sin(angle);

                xMin = Math.Min(xMin, x);
                xMax = Math.Max(xMax, x);
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);

                angle += angleInner;
            }

            angle = 3 * Math.PI / 2 + angleInner / 2;

            for (int side = 0; side < Sides; side++)
            {
                vertices.Add(new Point3D(0, 0, 0));
                textures.Add(new Point(-xMin / (xMax - xMin), yMax / (yMax - yMin)));
                normals.Add(normal);

                double x = Math.Cos(angle);
                double y = Math.Sin(angle);
                vertices.Add(new Point3D(x, y, 0));
                textures.Add(new Point((x - xMin) / (xMax - xMin),
                                       (yMax - y) / (yMax - yMin)));
                normals.Add(normal);

                angle += angleInner;
                x = Math.Cos(angle);
                y = Math.Sin(angle);
                vertices.Add(new Point3D(x, y, 0));
                textures.Add(new Point((x - xMin) / (xMax - xMin),
                                       (yMax - y) / (yMax - yMin)));
                normals.Add(normal);

                int index = vertices.Count - 3;
                indices.Add(index);
                indices.Add(index + 1);
                indices.Add(index + 2);
                
                if (Slices > 1)
                    TriangleSubdivide(vertices, normals, indices, textures);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new PolygonMesh();
        }
    }
}
