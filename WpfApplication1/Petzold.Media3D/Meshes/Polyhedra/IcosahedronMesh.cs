//------------------------------------------------
// IcosahedronMesh.cs (c) 2007 by Charles Petzold
//------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class IcosahedronMesh : PolyhedronMeshBase
    {
        static readonly double G = (1 + Math.Sqrt(5)) / 2;

        static readonly Point3D[,] faces = new Point3D[20, 3]
        {
            { new Point3D(0, G, 1), new Point3D(-1, 0, G), new Point3D(1, 0, G) },
            { new Point3D(-1, 0, G), new Point3D(0, -G, 1), new Point3D(1, 0, G) },

            { new Point3D(0, G, 1), new Point3D(1, 0, G), new Point3D(G, 1, 0) },
            { new Point3D(1, 0, G), new Point3D(G, -1, 0), new Point3D(G, 1, 0) },

            { new Point3D(0, G, 1), new Point3D(G, 1, 0), new Point3D(0, G, -1) },
            { new Point3D(G, 1, 0), new Point3D(1, 0, -G), new Point3D(0, G, -1) },

            { new Point3D(0, G, 1), new Point3D(0, G, -1), new Point3D(-G, 1, 0) },
            { new Point3D(0, G, -1), new Point3D(-1, 0, -G), new Point3D(-G, 1, 0) },

            { new Point3D(0, G, 1), new Point3D(-G, 1, 0), new Point3D(-1, 0, G) },
            { new Point3D(-G, 1, 0), new Point3D(-G, -1, 0), new Point3D(-1, 0, G) },

            { new Point3D(1, 0, G), new Point3D(0, -G, 1), new Point3D(G, -1, 0) },
            { new Point3D(0, -G, 1), new Point3D(0, -G, -1), new Point3D(G, -1, 0) },

            { new Point3D(G, 1, 0), new Point3D(G, -1, 0), new Point3D(1, 0, -G) },
            { new Point3D(G, -1, 0), new Point3D(0, -G, -1), new Point3D(1, 0, -G) },

            { new Point3D(0, G, -1), new Point3D(1, 0, -G), new Point3D(-1, 0, -G) },
            { new Point3D(1, 0, -G), new Point3D(0, -G, -1), new Point3D(-1, 0, -G) },

            { new Point3D(-G, 1, 0), new Point3D(-1, 0, -G), new Point3D(-G, -1, 0) },
            { new Point3D(-1, 0, -G), new Point3D(0, -G, -1), new Point3D(-G, -1, 0) },

            { new Point3D(-1, 0, G), new Point3D(-G, -1, 0), new Point3D(0, -G, 1) },
            { new Point3D(-G, -1, 0), new Point3D(0, -G, -1), new Point3D(0, -G, 1) },

        };

        public IcosahedronMesh()
        {
            // Set TextureCoordinates to default values.
            PointCollection textures = TextureCoordinates;
            TextureCoordinates = null;

            textures.Add(new Point(1, 0));
            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 1));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(0, 1));
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
