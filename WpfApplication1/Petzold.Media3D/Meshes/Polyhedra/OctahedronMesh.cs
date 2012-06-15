//-----------------------------------------------
// OctahedronMesh.cs (c) 2007 by Charles Petzold
//-----------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class OctahedronMesh : PolyhedronMeshBase
    {
        static readonly Point3D[,] faces = new Point3D[8, 3]
        {
            // front upper right
            { new Point3D(0, 1, 0), new Point3D(0, 0, 1), new Point3D(1, 0, 0) },

            // front upper left
            { new Point3D(0, 1, 0), new Point3D(-1, 0, 0), new Point3D(0, 0, 1) },

            // front lower left
            { new Point3D(0, -1, 0), new Point3D(0, 0, 1), new Point3D(-1, 0, 0) },

            // front lower right
            { new Point3D(0, -1, 0), new Point3D(1, 0, 0), new Point3D(0, 0, 1) },

            // back lower right
            { new Point3D(0, -1, 0), new Point3D(0, 0, -1), new Point3D(1, 0, 0) },

            // back lower left
            { new Point3D(0, -1, 0), new Point3D(-1, 0, 0), new Point3D(0, 0, -1) },

            // back upper left
            { new Point3D(0, 1, 0), new Point3D(0, 0, -1), new Point3D(-1, 0, 0) },

            // back upper right
            { new Point3D(0, 1, 0), new Point3D(1, 0, 0), new Point3D(0, 0, -1) }
        };

        public OctahedronMesh()
        {
            // Set TextureCoordinates to default values.
            PointCollection textures = TextureCoordinates;
            TextureCoordinates = null;

            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 1));
            textures.Add(new Point(1, 0));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(0, 1));
            textures.Add(new Point(1, 1));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 1));
            textures.Add(new Point(0, 1));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 0));
            textures.Add(new Point(1, 1));

            TextureCoordinates = textures;
        }

        protected override Point3D[,] Faces
        {
            get
            {
                return faces;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new OctahedronMesh();
        }
    }
}
