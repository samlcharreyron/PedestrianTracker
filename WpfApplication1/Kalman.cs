using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PedestrianTracker
{
    /// <summary>
    /// Used to apply a 4-state Kalman filter tracking constant velocity
    /// </summary>
    public class Kalman
    {
        Matrix Identity4 = new Matrix(4, 4)
        {
            Data = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }
        };

        Matrix Identity2 = new Matrix(2, 2)
        {
            Data = new double[] { 1, 0, 0, 1 }
        };
        
        /// <summary>
        /// States
        /// </summary>
        Matrix X = new Matrix(1,4);

        /// <summary>
        /// State space model
        /// </summary>
        Matrix F = new Matrix(4, 4);

        /// <summary>
        /// Covariance matrix
        /// </summary>
        Matrix P = new Matrix(4, 4);

        /// <summary>
        /// Minimum covariance: associated with process noise
        /// </summary>
        Matrix Q = new Matrix(4, 4);

        /// <summary>
        /// Minimal innovative covariance: associated with measurement noise
        /// </summary>
        Matrix R = new Matrix(2, 2);

        /// <summary>
        /// Ouput matrix
        /// </summary>
        Matrix H = new Matrix(4, 2);

        /// <summary>
        /// the last computed x position
        /// </summary>
        public double x
        {
            get { return X.Data[0]; }
            set { X.Data[0] = value; }
        }

        /// <summary>
        /// the last computed z position
        /// </summary>
        public double z
        {
            get { return X.Data[1]; }
            set { X.Data[1] = value; }
        }


        /// <summary>
        /// the last computed velocity in x
        /// </summary>
        public double vx
        {
            get { return X.Data[2]; }
            set { X.Data[2] = value; }
        }

        /// <summary>
        /// the last computed velocity in z
        /// </summary>
        public double vz
        {
            get { return X.Data[3]; }
            set { X.Data[3] = value; }
        }


        #region Prediction
        /// <summary>
        /// Predict the next values of states using state space model
        /// </summary>
        /// <param name="dt">the time step to predict forward by</param>
        /// <returns>Vector X~ of predicted states</returns>
        public Matrix Prediction(double dt)
        {
            Matrix result = new Matrix(4, 1);

            result.Data[0] = X.Data[0] + X.Data[2] * dt / 1000;
            result.Data[1] = X.Data[1] + X.Data[3] * dt / 1000;
            result.Data[2] = X.Data[2];
            result.Data[3] = X.Data[3];

            return X = result;
        }
 
        /// <summary>
        /// Predict the next values of the covariance matrix
        /// </summary>
        /// <param name="dt">the time step to predict forward by</param>
        /// <returns>Matrix P~ of predicted covariance</returns>
        public Matrix Covariance(double dt)
        {

            try
            {
                ReloadStateModel(dt);
                return P = Matrix.Add(Matrix.MultiplyABAT(F, P), Q);
            }
            catch
            {
              throw new ArithmeticException("Unable to obtain covariance");
            }
        }

        #endregion

        public Matrix update(double vx_new, double vz_new)
        {
            Matrix Y = new Matrix(2,2){
                Data = new double[] {vx_new - vx, vz_new - vz}
            };

            //Updating X
            Matrix S = Matrix.Add(Matrix.MultiplyABAT(H, P), R);
            Matrix Ht = Matrix.Transpose(H);
            Matrix Sinv = Matrix.Invert(S);
            Matrix K = Matrix.Multiply(P, Ht);

            if (Sinv != null)
            {
                K.Multiply(Sinv);
            }
            //Matrix temp = Matrix.Multiply(K, Y);
            // K * Y
            Matrix temp = new Matrix(1, 4);
            temp.Data[0] = K.Get(0,0) * Y.Data[0] + K.Get(1,0) * Y.Data[1];
            temp.Data[1] = K.Get(0,1) * Y.Data[0] + K.Get(1,1) * Y.Data[1];
            temp.Data[2] = K.Get(0,2) * Y.Data[0] + K.Get(1,2) * Y.Data[1];
            temp.Data[3] = K.Get(0, 3) * Y.Data[0] + K.Get(1, 3) * Y.Data[1];

            X.Add(temp);

            //Updating P
            Matrix I = new Matrix(4, 4);
            I = Identity4.Clone();
            Matrix kh = Matrix.Multiply(K,H);
            kh.Multiply(-1);
            I.Add(kh);
            P = Matrix.Multiply(I,P);

            return X;
        }
         
        private void ReloadStateModel(double dt)
        {
            Matrix model = new Matrix(4, 4);
            model.Set(0, 0, 1);
            model.Set(1, 1, 1);
            model.Set(2, 2, 1);
            model.Set(3, 3, 1);
            model.Set(2, 0, dt / 1000);
            model.Set(3, 1, dt / 1000);

            this.F = model.Clone();
        }

        /// <summary>
        /// Called to reset Kalman filter when first run
        /// </summary>
        /// <param name="x">initial x position</param>
        /// <param name="z">initial z position</param>
        /// <param name="vx">initial velocity (x direction)</param>
        /// <param name="vz">initial position (z direction)</param>
        /// <param name="dt">initial time step (s)</param>
        public void Reset(double x, double z, double vx, double vz, double dt)
        {
            //Set state space model
            ReloadStateModel(dt);

            //Set minimum covariance matrix
            Q = Identity4.Clone();
            Q.Multiply(0.001);

            //Set measurement covariance matrix
            R = Identity2.Clone();
            R.Multiply(0.1);

            //Set initial P matrix
            P = Identity4.Clone();

            //Set initial states
            X.Data = new double[]{x,z,vx,vz};

            //Set output matrix
            H.Set(2, 0, 1);
            H.Set(3, 1, 1);
        }
    }
}
