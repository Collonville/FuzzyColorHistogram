using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FuzzyColorHistogram1
{
    class ClusterCentroid
    {
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="x">Centroid x-coord</param>
        /// <param name="PixelCount">The number of pixels in the cluster, used to find the average</param>
        /// <param name="MembershipSum">The sum of all the cluster pixels Red values, used to find the average</param>

        public double X { get; set; }
        public double PixelCount { get; set; }
        public double VectorSum { get; set; }
        public double MembershipSum { get; set; }
        public double Length { get; set; }

        public ClusterCentroid(double x, double col)
        {
            this.X = x;
            this.VectorSum = 0;
            this.PixelCount = 0;
            this.MembershipSum = 0;
            this.Length = col;
        }
    }
}
