using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FuzzyColorHistogram1
{
    class XYZ
    {
        private static Matrix<double> sRGB_D65Matrix = DenseMatrix.OfArray(new double[,] {
            {0.412391, 0.357584, 0.180481},
            {0.212639, 0.715169, 0.072192},
            {0.019331, 0.119195, 0.950532}
        });
        private static Matrix<double> AdobeRGB_D65Matrix = DenseMatrix.OfArray(new double[,] {
            {0.576669, 0.185558, 0.188229},
            {0.297345, 0.627364, 0.075291},
            {0.027031, 0.070689, 0.991338}
        });

        public static Vector<double> RGB2XYZ(Vector<double> rgb)
        {
            return (sRGB_D65Matrix * (rgb / 255.0));
        }

        public static Vector<double> RGB2XYZ(Vector<double> rgb, string standard)
        {
            Vector<double> XYZ = new DenseVector(3);
            if (standard.Equals("sRGB"))
            {
                rgb /= 255.0;

                rgb[0] = GammaCorrection_sRGB(rgb[0]);
                rgb[1] = GammaCorrection_sRGB(rgb[1]);
                rgb[2] = GammaCorrection_sRGB(rgb[2]);

                XYZ[0] = 0.412391 * rgb[0] + 0.357584 * rgb[1] + 0.180481 * rgb[2];
                XYZ[1] = 0.212639 * rgb[0] + 0.715169 * rgb[1] + 0.072192 * rgb[2];
                XYZ[2] = 0.019331 * rgb[0] + 0.119195 * rgb[1] + 0.950532 * rgb[2];
            }
            else if (standard.Equals("Adobe RGB"))
            {

            }

            return XYZ;
        }

        private static double GammaCorrection_sRGB(double val)
        {
            if (val <= 0.040450)
            {
                return (val / 12.92);
            }
            else
            {
                return Math.Pow(((val + 0.055) / 1.055), 2.4);
            }
        }
    }
}
