//-----------------------------------------------------
// AlgorithmicTransform.cs (c) 2007 by Charles Petzold
//-----------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AlgorithmicTransform : Animatable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        public abstract void Transform(Point3DCollection points);
    }
}