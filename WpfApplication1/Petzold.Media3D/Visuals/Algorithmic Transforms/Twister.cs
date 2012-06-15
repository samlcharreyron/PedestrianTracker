//----------------------------------------
// Twister.cs (c) 2007 by Charles Petzold
//
// Part of Petzold.Media3D.dll
//
// Note: No PropertyChanged event handlers are required in this class
//  because instances are always children of an 
//  AlgorithmicTransformCollection, and that generates changes
//  notifications on its own.
//---------------------------------------- 
using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class Twister : AlgorithmicTransform
    {
        // Private fields for Transform method.
        // ------------------------------------
        AxisAngleRotation3D axisRotate;
        RotateTransform3D xform;

        // Constructor just sets private fields.
        // -------------------------------------
        public Twister()
        {
            axisRotate = new AxisAngleRotation3D();
            xform = new RotateTransform3D(axisRotate);
        }

        // Axis property (probably shouldn't be (0, 0, 0) but we'll let it go).
        // --------------------------------------------------------------------
        public static readonly DependencyProperty AxisProperty =
            DependencyProperty.Register("Axis", 
                typeof(Vector3D), typeof(Twister),
                new PropertyMetadata(new Vector3D(0, 1, 0)));

        public Vector3D Axis
        {
            set { SetValue(AxisProperty, value); }
            get { return (Vector3D)GetValue(AxisProperty); }
        }

        // Center property.
        // ----------------
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", 
                typeof(Point3D), typeof(Twister),
                new PropertyMetadata(new Point3D(0, 0, 0)));

        public Point3D Center
        {
            set { SetValue(CenterProperty, value); }
            get { return (Point3D)GetValue(CenterProperty); }
        }

        // Angle property.
        // ---------------
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", 
                typeof(double), typeof(Twister),
                new PropertyMetadata(0.0));

        public double Angle
        {
            set { SetValue(AngleProperty, value); }
            get { return (double)GetValue(AngleProperty); }
        }

        // Attenuation property (must be greater than zero).
        // -------------------------------------------------
        public static readonly DependencyProperty AttenuationProperty =
            DependencyProperty.Register("Attenuation", 
                typeof(double), typeof(Twister),
                new PropertyMetadata(1.0), ValidateAttenuation);

        public double Attenuation
        {
            set { SetValue(AttenuationProperty, value); }
            get { return (double)GetValue(AttenuationProperty); }
        }

        static bool ValidateAttenuation(object obj)
        {
            return (double)obj > 0;
        }

        // Required CreateInstanceCore method when inheriting from Freezable.
        // ------------------------------------------------------------------
        protected override Freezable CreateInstanceCore()
        {
            return new Twister();
        }

        // Required Transform method when inheriting from AlgorithmicTransform.
        // --------------------------------------------------------------------
        public override void Transform(Point3DCollection points)
        {
            xform.CenterX = Center.X;
            xform.CenterY = Center.Y;
            xform.CenterZ = Center.Z;

            axisRotate.Axis = Axis;
            Vector3D axisNormalized = Axis;
            axisNormalized.Normalize();
            double angleAttenuated = Angle / Attenuation;

            for (int i = 0; i < points.Count; i++)
            {
                axisRotate.Angle = angleAttenuated *
                    Vector3D.DotProduct(axisNormalized, points[i] - Center);

                points[i] = xform.Transform(points[i]);
            }
        }
    }
}
