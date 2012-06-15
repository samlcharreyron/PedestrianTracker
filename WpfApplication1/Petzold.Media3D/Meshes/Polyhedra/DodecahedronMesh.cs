//-------------------------------------------------
// DodecahedronMesh.cs (c) 2007 by Charles Petzold
//-------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class DodecahedronMesh : PolyhedronMeshBase
    {
        static readonly double G = (1 + Math.Sqrt(5)) / 2;      // approximately 1.618
        static readonly double H = 1 / G;                       // approximately 0.618
        static readonly double A = (3 * H + 4) / 5;             // approximately 1.171
        static readonly double B = (2 + G) / 5;                 // approximately 0.724

        static readonly Point3D[,] faces = new Point3D[12, 6]
        {
            { new Point3D( A, -B, 0), new Point3D( 1, -1, -1), new Point3D( G, 0, -H), new Point3D( G, 0,  H), new Point3D( 1, -1,  1), new Point3D( H, -G, 0) },
            { new Point3D(-A, -B, 0), new Point3D(-1, -1,  1), new Point3D(-G, 0,  H), new Point3D(-G, 0, -H), new Point3D(-1, -1, -1), new Point3D(-H, -G, 0) },
            { new Point3D(-A,  B, 0), new Point3D(-1,  1, -1), new Point3D(-G, 0, -H), new Point3D(-G, 0,  H), new Point3D(-1,  1,  1), new Point3D(-H,  G, 0) },
            { new Point3D( A,  B, 0), new Point3D( 1,  1,  1), new Point3D( G, 0,  H), new Point3D( G, 0, -H), new Point3D( 1,  1, -1), new Point3D( H,  G, 0) },
            { new Point3D(-B, 0, -A), new Point3D(-1,  1, -1), new Point3D(0,  H, -G), new Point3D(0, -H, -G), new Point3D(-1, -1, -1), new Point3D(-G, 0, -H) },
            { new Point3D(-B, 0,  A), new Point3D(-1, -1,  1), new Point3D(0, -H,  G), new Point3D(0,  H,  G), new Point3D(-1,  1,  1), new Point3D(-G, 0,  H) },
            { new Point3D( B, 0, -A), new Point3D( 1, -1, -1), new Point3D(0, -H, -G), new Point3D(0,  H, -G), new Point3D( 1,  1, -1), new Point3D( G, 0, -H) },
            { new Point3D( B, 0,  A), new Point3D( 1,  1,  1), new Point3D(0,  H,  G), new Point3D(0, -H,  G), new Point3D( 1, -1,  1), new Point3D( G, 0,  H) },
            { new Point3D(0, -A, -B), new Point3D( 1, -1, -1), new Point3D( H, -G, 0), new Point3D(-H, -G, 0), new Point3D(-1, -1, -1), new Point3D(0, -H, -G) },
            { new Point3D(0,  A, -B), new Point3D(-1,  1, -1), new Point3D(-H,  G, 0), new Point3D( H,  G, 0), new Point3D( 1,  1, -1), new Point3D(0,  H, -G) },
            { new Point3D(0, -A,  B), new Point3D(-1, -1,  1), new Point3D(-H, -G, 0), new Point3D( H, -G, 0), new Point3D( 1, -1,  1), new Point3D(0, -H,  G) },
            { new Point3D(0,  A,  B), new Point3D( 1,  1,  1), new Point3D( H,  G, 0), new Point3D(-H,  G, 0), new Point3D(-1,  1,  1), new Point3D(0,  H,  G) }
        };

        public DodecahedronMesh()
        {
            // Set TextureCoordinates to default values.
            PointCollection textures = TextureCoordinates;
            TextureCoordinates = null;

            textures.Add(new Point(0.5, 0.5));
            textures.Add(new Point(0.5, 0));
            textures.Add(new Point(1, 0.4));
            textures.Add(new Point(0.85, 1));
            textures.Add(new Point(0.15, 1));
            textures.Add(new Point(0, 0.4));

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
