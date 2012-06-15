using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PedestrianTracker
{
    class RoadAxisY1PaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                return (double)value - double.Parse((string)parameter);
            }

            else return 460.0;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class RoadAxisY2PaddingConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double y2 = 460;

            y2 = /*RenderHeight*/ (double)value[0] - /*Padding*/ double.Parse((string)parameter) - /*RenderWidth*/ (double)value[1]  * Math.Tan((/*KinectAngle*/(int)value[2] * Math.PI) / 180.0);
            
            return y2;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
