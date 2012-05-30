using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace PedestrianTracker
{
     public class MyRender : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
                

                drawingContext.DrawEllipse(Brushes.Blue, null, new Point(0,0), 1.0, 1.0);
            }
        }
}

