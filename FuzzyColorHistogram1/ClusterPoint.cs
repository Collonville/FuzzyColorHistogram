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
    class ClusterPoint
    {
        /// <summary>
        /// Gets or sets the RGB color of the point
        /// </summary>
        public Vector<double> RawVector { get; set; }

        /// <summary>
        /// Gets or sets the original RGB color of the point
        /// </summary>

        public int Height { get; set; }

        public int Width { get; set; }

 
        /// <summary>
        /// Gets or sets cluster index, the strongest membership value to a cluster, used for defuzzification
        /// </summary>
        public double ClusterIndex { get; set; }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        /// <param name="z">PixelColor</param>
        /// <param name="z">OriginalPixelColor</param>
        /// <param name="z">ClusterIndex</param>
        public ClusterPoint(Vector<double> col)
		{
            this.RawVector = col;
            this.ClusterIndex = -1;
		}

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        public ClusterPoint(Vector<double> z, object tag)
        {
            this.RawVector = z;
            this.ClusterIndex = -1;
        }
    }
}
