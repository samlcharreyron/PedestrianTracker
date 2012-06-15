//---------------------------------------------------------------
// AlgorithmicTransformCollection.cs (c) 2007 by Charles Petzold
//---------------------------------------------------------------
using System.Windows;

namespace Petzold.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public class AlgorithmicTransformCollection :
                                        FreezableCollection<AlgorithmicTransform>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new AlgorithmicTransformCollection();
        }
    }
}
