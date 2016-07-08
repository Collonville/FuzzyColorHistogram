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

        public static Vector<double> RGBtoXYZ(Vector<double> rgb)
        {
            return (sRGB_D65Matrix * (rgb / 255.0));
        }

        public static Vector<double> RGBtoXYZ(Vector<double> rgb, string standard)
        {
            //TODO:ガンマ補正の式を入れる
            if (standard.Equals("sRGB"))
            {

            }
            else if (standard.Equals("Adobe RGB"))
            {

            }
            return (sRGB_D65Matrix * (rgb / 255.0));
        }
    }
}
