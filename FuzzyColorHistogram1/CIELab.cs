using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FuzzyColorHistogram1
{
    class CIELab
    {
        private static Vector<double> WhitePoint_D50 = new DenseVector(new double[] { 0.9642, 1.0, 0.8249 });
        private static Vector<double> WhitePoint_D65 = new DenseVector(new double[] { 0.9504, 1.0, 1.08906 });

        private static double f(double t)
        {
            if (t > Math.Pow(6.0 / 29.0, 3))
            {
                return Math.Pow(t, 1.0 / 3.0);
            } 
            else
            {
                return (((Math.Pow(29.0 / 3.0, 3.0) * t) + 16) / 116.0);
            }
        }

        public static Vector<double> XYZtoLab(Vector<double> xyz)
        {
            Vector<double> Lab = new DenseVector(3);

            Lab[0] = 116.0 * f(xyz[1] / WhitePoint_D50[1]) - 16.0;
            Lab[1] = 500.0 * (f(xyz[0] / WhitePoint_D50[0]) - f(xyz[1] / WhitePoint_D50[1])) / 100;
            Lab[2] = 200.0 * (f(xyz[1] / WhitePoint_D50[1]) - f(xyz[2] / WhitePoint_D50[2])) / 100;

            return Lab;
        }
    }
}
