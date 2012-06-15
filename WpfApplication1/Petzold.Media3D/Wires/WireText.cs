//-----------------------------------------
// WireText.cs (c) 2007 by Charles Petzold
//-----------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public class WireText : WireBase
    {
        TextGenerator txtgen = new TextGenerator();

        /// <summary>
        ///     Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", 
            typeof(string), 
            typeof(WireText),
            new UIPropertyMetadata("", PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            set { SetValue(TextProperty, value); }
            get { return (string)GetValue(TextProperty); }
        }

        /// <summary>
        ///     Identifies the Font dependency property.
        /// </summary>
        public static readonly DependencyProperty FontProperty =
            DependencyProperty.Register("Font",
            typeof(Font),
            typeof(WireText),
            new UIPropertyMetadata(Font.Modern, PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public Font Font
        {
            set { SetValue(FontProperty, value); }
            get { return (Font)GetValue(FontProperty); }
        }

        /// <summary>
        ///     Identifies the FontSize dependency property. 
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", 
            typeof(double), 
            typeof(WireText),
            new UIPropertyMetadata(0.10, PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public double FontSize
        {
            set { SetValue(FontSizeProperty, value); }
            get { return (double)GetValue(FontSizeProperty); }
        }

        /// <summary>
        ///     Identifies the Origin dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", 
            typeof(Point3D), 
            typeof(WireText),
            new UIPropertyMetadata(new Point3D(), 
                                   PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public Point3D Origin
        {
            set { SetValue(OriginProperty, value); }
            get { return (Point3D)GetValue(OriginProperty); }
        }

        /// <summary>
        ///     Identifies the BaselineDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty BaselineDirectionProperty =
            DependencyProperty.Register("BaselineDirection", 
            typeof(Vector3D), 
            typeof(WireText),
            new UIPropertyMetadata(new Vector3D(1, 0, 0), 
                                   PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public Vector3D BaselineDirection
        {
            set { SetValue(BaselineDirectionProperty, value); }
            get { return (Vector3D)GetValue(BaselineDirectionProperty); }
        }

        /// <summary>
        ///     Identifies the UpDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty UpDirectionProperty =
            DependencyProperty.Register("UpDirection", 
            typeof(Vector3D), 
            typeof(WireText),
            new UIPropertyMetadata(new Vector3D(0, 1, 0),
                                   PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public Vector3D UpDirection
        {
            get { return (Vector3D)GetValue(UpDirectionProperty); }
            set { SetValue(UpDirectionProperty, value); }
        }

        /// <summary>
        ///     Identifies the HorizontalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register("HorizontalAlignment", 
            typeof(HorizontalAlignment), 
            typeof(WireText),
            new UIPropertyMetadata(HorizontalAlignment.Left,
                                   PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            set { SetValue(HorizontalAlignmentProperty, value); }
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
        }

        /// <summary>
        ///     Identifies the <c>VerticalAlignment</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalAlignmentProperty =
            DependencyProperty.Register("VerticalAlignment", 
            typeof(VerticalAlignment), 
            typeof(WireText),
            new UIPropertyMetadata(VerticalAlignment.Top,
                                   PropertyChanged, null, true));

        /// <summary>
        /// 
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            set { SetValue(VerticalAlignmentProperty, value); }
            get { return (VerticalAlignment)GetValue(VerticalAlignmentProperty); }
        }

        protected override void Generate(DependencyPropertyChangedEventArgs args,
                                         Point3DCollection lines)
        {
            lines.Clear();

            txtgen.Font = Font;
            txtgen.FontSize = FontSize;
            txtgen.Rounding = Rounding;
            txtgen.Thickness = Thickness;
            txtgen.BaselineDirection = BaselineDirection;
            txtgen.Origin = Origin;
            txtgen.UpDirection = UpDirection;
            txtgen.VerticalAlignment = VerticalAlignment;
            txtgen.HorizontalAlignment = HorizontalAlignment;

            txtgen.Generate(lines, Text);
        }
    }
}



