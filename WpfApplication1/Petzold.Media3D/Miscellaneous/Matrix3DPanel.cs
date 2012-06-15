//----------------------------------------------
// Matrix3DPanel.cs (c) 2007 by Charles Petzold
//----------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public class Matrix3DPanel : FrameworkElement
    {
        UniformGrid unigrid;

        /// <summary>
        /// 
        /// </summary>
        public Matrix3DPanel()
        {
            string[] strIds = { "M11", "M12", "M13", "M14",
                                "M21", "M22", "M23", "M24",
                                "M31", "M32", "M33", "M34",
                                "OffsetX", "OffsetY", "OffsetZ", "M44" };

            NameScope.SetNameScope(this, new NameScope());

            // Create UniformGrid and add it as a child.
            unigrid = new UniformGrid();
            unigrid.Rows = 4;
            unigrid.Columns = 4;
            AddVisualChild(unigrid);
            AddLogicalChild(unigrid);

            for (int i = 0; i < strIds.Length; i++)
            {
                // StackPanel for each cell.
                StackPanel stack1 = new StackPanel();
                stack1.Orientation = Orientation.Vertical;
                stack1.Margin = new Thickness(12);
                unigrid.Children.Add(stack1);

                // StackPanel for TextBlock elements.
                StackPanel stack2 = new StackPanel();
                stack2.Orientation = Orientation.Horizontal;
                stack2.HorizontalAlignment = HorizontalAlignment.Center;
                stack1.Children.Add(stack2);

                // ScrollBar for each cell.
                ScrollBar scroll = new ScrollBar();
                scroll.Orientation = Orientation.Horizontal;
                scroll.Value = (i % 5 == 0) ? 1 : 0;
                scroll.SmallChange = 0.01;
                scroll.LargeChange = 0.1;
                scroll.Focusable = true;
                stack1.Children.Add(scroll);

                RegisterName(strIds[i], scroll);

                // Set bindings for scrollbars.
                Binding binding = new Binding("Minimum");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                scroll.SetBinding(ScrollBar.MinimumProperty, binding);

                binding = new Binding("Maximum");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                scroll.SetBinding(ScrollBar.MaximumProperty, binding);

                // TextBlock elements to show values.
                TextBlock txtblk = new TextBlock();
                txtblk.Text = strIds[i] + " = ";
                stack2.Children.Add(txtblk);

                binding = new Binding("Value");
                binding.Source = scroll;
                binding.Mode = BindingMode.OneWay;

                txtblk = new TextBlock();
                txtblk.SetBinding(TextBlock.TextProperty, binding);
                stack2.Children.Add(txtblk);
            }

            AddHandler(ScrollBar.ValueChangedEvent, new RoutedEventHandler(ScrollBarOnValueChanged));
        }

        // Dependency property key for Matrix.
        static readonly DependencyPropertyKey MatrixKey =
            DependencyProperty.RegisterReadOnly("Matrix",
                typeof(Matrix3D ),
                typeof(Matrix3DPanel),
                new PropertyMetadata(new Matrix3D()));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty MatrixProperty =
            MatrixKey.DependencyProperty;

        /// <summary>
        /// 
        /// </summary>
        public Matrix3D Matrix
        {
            private set { SetValue(MatrixKey, value); }
            get { return (Matrix3D)GetValue(MatrixProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            ScrollBar.MinimumProperty.AddOwner(
                typeof(Matrix3DPanel),
                new PropertyMetadata(-3.0));

        /// <summary>
        /// 
        /// </summary>
        public double Minimum
        {
            set { SetValue(MinimumProperty, value); }
            get { return (double)GetValue(MinimumProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            ScrollBar.MaximumProperty.AddOwner(
                typeof(Matrix3DPanel),
                new PropertyMetadata(3.0));

        /// <summary>
        /// 
        /// </summary>
        public double Maximum
        {
            set { SetValue(MaximumProperty, value); }
            get { return (double)GetValue(MaximumProperty); }
        }

        // Set Matrix from ScrollBar values.
        void ScrollBarOnValueChanged(object sender, RoutedEventArgs args)
        {
            Matrix3D matx = new Matrix3D();

            matx.M11 = (FindName("M11") as ScrollBar).Value;
            matx.M12 = (FindName("M12") as ScrollBar).Value;
            matx.M13 = (FindName("M13") as ScrollBar).Value;
            matx.M14 = (FindName("M14") as ScrollBar).Value;

            matx.M21 = (FindName("M21") as ScrollBar).Value;
            matx.M22 = (FindName("M22") as ScrollBar).Value;
            matx.M23 = (FindName("M23") as ScrollBar).Value;
            matx.M24 = (FindName("M24") as ScrollBar).Value;

            matx.M31 = (FindName("M31") as ScrollBar).Value;
            matx.M32 = (FindName("M32") as ScrollBar).Value;
            matx.M33 = (FindName("M33") as ScrollBar).Value;
            matx.M34 = (FindName("M34") as ScrollBar).Value;

            matx.OffsetX = (FindName("OffsetX") as ScrollBar).Value;
            matx.OffsetY = (FindName("OffsetY") as ScrollBar).Value;
            matx.OffsetZ = (FindName("OffsetZ") as ScrollBar).Value;
            matx.M44 = (FindName("M44") as ScrollBar).Value;

            Matrix = matx;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index > 0)
                throw new ArgumentException("index");

            return unigrid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            unigrid.Measure(availableSize);
            return unigrid.DesiredSize;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            unigrid.Arrange(new Rect(new Point(0, 0), finalSize));
            return finalSize;
        }
    }
}
