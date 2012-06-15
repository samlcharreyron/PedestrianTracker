//----------------------------------------------------
// TeapotTriangleRange.cs (c) 2007 by Charles Petzold
//----------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Petzold.Media3D
{
    [TypeConverter(typeof(TeapotTriangleRangeConverter))]
    public class TeapotTriangleRange : Animatable
    {
        /// <summary>
        ///     Identifies the Begin dependency property.
        /// </summary>
        public static readonly DependencyProperty BeginProperty =
            DependencyProperty.Register("Begin",
                typeof(int),
                typeof(TeapotTriangleRange),
                new PropertyMetadata(0));

        public int Begin
        {
            set { SetValue(BeginProperty, value); }
            get { return (int)GetValue(BeginProperty); }
        }

        /// <summary>
        ///     Identifies the End dependency property.
        /// </summary>
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End",
                typeof(int),
                typeof(TeapotTriangleRange),
                new PropertyMetadata(2255));

        public int End
        {
            set { SetValue(EndProperty, value); }
            get { return (int)GetValue(EndProperty); }
        }

        public TeapotTriangleRange()
        {
        }

        public TeapotTriangleRange(int begin, int end)
        {
            Begin = begin;
            End = end;
        }

        public static TeapotTriangleRange All
        {
            get { return new TeapotTriangleRange(); }
        }

        public static TeapotTriangleRange Pot
        {
            get { return new TeapotTriangleRange(0, 1703); }
        }

        public static TeapotTriangleRange Body
        {
            get { return new TeapotTriangleRange(0, 1127); }
        }

        public static TeapotTriangleRange Handle
        {
            get { return new TeapotTriangleRange(1128, 1415); }
        }

        public static TeapotTriangleRange Spout
        {
            get { return new TeapotTriangleRange(1416, 1703); }
        }

        public static TeapotTriangleRange Lid
        {
            get { return new TeapotTriangleRange(1704, 2255); }
        }

        public static TeapotTriangleRange Parse(string str)
        {
            string[] strTokens = str.Split(' ', ',');
            int num = 0;
            int[] values = new int[2];

            foreach (string strToken in strTokens)
            {
                if (strToken.Length > 0)
                {
                    if (num == 2)
                        throw new FormatException("Too many tokens in string.");

                    values[num++] = Int32.Parse(strToken);
                }
            }

            if (num != 2)
                throw new FormatException("Not enough tokens in string.");

            return new TeapotTriangleRange(values[0], values[1]);
        }

        public override string ToString()
        {
            return String.Format("{0},{1}", Begin, End);
        }

        /// <summary>
        ///     Creates a new instance of the TeapotTriangleRange class.
        /// </summary>
        /// <returns>
        ///     A new instance of TeapotTriangleRange.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new SphereMesh();
        }

    }
}
