//-------------------------------------------------------------
// TeapotTriangleRangeConverter.cs (c) 2007 by Charles Petzold
//-------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Petzold.Media3D
{
    /// <summary>
    ///     Converts instances of other types to and from a 
    ///     TeapotTriangleRange. 
    /// </summary>
    public class TeapotTriangleRangeConverter : TypeConverter
    {
        /// <summary>
        ///     Indicates whether an object can be converted from a given 
        ///     type to an instance of a TeapotTriangleRange. 
        /// </summary>
        /// <param name="context">
        ///     Describes the context information of a type.
        /// </param>
        /// <param name="sourceType">
        ///     The source Type that is being queried for conversion support.
        /// </param>
        /// <returns>
        ///     true if object of the specified type can be converted to a 
        ///     TeapotTriangleRange; otherwise, false.  
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, 
                                            Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        ///     Determines whether instances of TeapotTriangleRange can be 
        ///     converted to the specified type. 
        /// </summary>
        /// <param name="context">
        ///     Describes the context information of a type.
        /// </param>
        /// <param name="destinationType">
        ///     The desired type this TeapotTriangleRange is being evaluated 
        ///     to be converted to.
        /// </param>
        /// <returns>
        ///     true if instances of TeapotTriangleRange can be converted to 
        ///     destinationType; otherwise, false.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, 
                                          Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        ///     Converts the specified object to a TeapotTriangleRange. 
        /// </summary>
        /// <param name="context">
        ///     Describes the context information of a type.
        /// </param>
        /// <param name="culture">
        ///     Describes the CultureInfo of the type being converted. 
        /// </param>
        /// <param name="value">
        ///     The object being converted.
        /// </param>
        /// <returns>
        ///     The TeapotTriangleRange created from converting value. 
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, 
                                           CultureInfo culture, 
                                           object value)
        {
            if (value.GetType() != typeof(string))
                return base.ConvertFrom(context, culture, value);

            string str = (value as string).Trim();
            PropertyInfo[] props = typeof(TeapotTriangleRange).GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo prop in props)
                if (prop.PropertyType == typeof(TeapotTriangleRange) &&
                        0 == String.Compare(str, prop.Name, true))
                    return prop.GetValue(null, null);

            return TeapotTriangleRange.Parse(value as string);
        }

        /// <summary>
        ///     Converts the specified TeapotTriangleRange to the specified type.
        /// </summary>
        /// <param name="context">
        ///     Describes the context information of a type.
        /// </param>
        /// <param name="culture">
        ///     Describes the CultureInfo of the type being converted.
        /// </param>
        /// <param name="value">
        ///     The TeapotTriangleRange to convert.
        /// </param>
        /// <param name="destinationType">
        ///     The type to convert the TeapotTriangleRange to.
        /// </param>
        /// <returns>
        ///     A new instance of the destinationType. 
        /// </returns>
        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value,
                                         Type destinationType)
        {
            if (destinationType == typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);
            
            return value.ToString();
        }
    }
}
