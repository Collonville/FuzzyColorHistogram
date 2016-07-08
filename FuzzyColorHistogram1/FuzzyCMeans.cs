using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using System.Drawing;


namespace FuzzyColorHistogram1
{
    class FuzzyCMeans
    {
        /// <summary>
        /// Array containing all points used by the algorithm
        /// </summary>
        private Vector<double> featureVector;

        /// <summary>
        /// Array containing all clusters handled by the algorithm
        /// </summary>
        private List<ClusterCentroid> Clusters = new List<ClusterCentroid>();

        /// <summary>
        /// Array containing all clusters membership value of all points to each cluster
        /// Fuzzy  rules state that the sum of the membership of a point to all clusters must be 1
        /// </summary>
        public double[,] U;

        private bool isConverged = false;

        public bool Converged { get { return isConverged; } }

        /// <summary>
        /// Gets or sets the current fuzzyness factor
        /// </summary>
        private double Fuzzyness;

        /// <summary>
        /// Algorithm precision
        /// </summary>
        private double Eps = Math.Pow(10, -5);

        /// <summary>
        /// Gets or sets objective function
        /// </summary>
        public double J { get; set; }

        private int numCluster;

        private int maxIterations = 500;
  
        /// <summary>
        /// Initialize the algorithm with points and initial clusters
        /// </summary>
        /// <param name="points">The list of Points objects</param>
        /// <param name="clusters">The list of Clusters objects</param>
        /// <param name="fuzzy">The fuzzyness factor to be used, constant</param>
        /// <param name="numCluster">The number of clusters requested by the user from the GUI</param>
        public FuzzyCMeans(Vector<double> points, double fuzzy, int numCluster)
        {
            this.numCluster = numCluster;
            featureVector = points;

            U = new double[this.featureVector.Count, numCluster];
            this.Fuzzyness = fuzzy;

            double diff;

            //Create random points to use a the cluster centroids
            Random random = new Random();
            for (int i = 0; i < numCluster; i++)
            {
                int randomNumber1 = random.Next(featureVector.Count);

                Clusters.Add(new ClusterCentroid(randomNumber1, featureVector[randomNumber1]));
            }

            // Iterate through all points to create initial U matrix
            for (int i = 0; i < this.featureVector.Count; i++)
            {
                double sum = 0.0;

                for (int j = 0; j < numCluster; j++)
                {
                    ClusterCentroid c = this.Clusters[j];
                    diff = Math.Sqrt(Math.Pow(CalculateEuclideanDistance(featureVector[i], c.Length), 2.0));
                    U[i, j] = (diff == 0) ? Eps : diff;
                    sum += U[i, j];
                }
             }

            this.RecalculateClusterMembershipValues();
        }

        public void runFCM()
        {
            int k = 0;
            do
            {
                k++;
                J = CalculateObjectiveFunction();
                CalculateClusterCentroids();
                Step();
                double Jnew = CalculateObjectiveFunction();
                Console.WriteLine("Run method i={0} accuracy = {1} delta={2}", k, J, Math.Abs(J - Jnew));

                // Format and display the TimeSpan value.
                // string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds / 10);
                // toolStripStatusLabel3.Text = "Duration: " + elapsedTime;

                //backgroundWorker.ReportProgress((100 * k) / maxIterations, "Iteration " + k);

                if (Math.Abs(J - Jnew) < Eps) 
                    break;
            }
            while (maxIterations > k);

            Console.WriteLine("Done...");
        }

        /// <summary>
        /// Recalculates the cluster membership values to ensure that 
        /// the sum of all membership values of a point to all clusters is 1
        /// </summary>
        private void RecalculateClusterMembershipValues() 
        {
            for (int i = 0; i < this.featureVector.Count; i++)
            {
               double max = 0.0;
               double min = 0.0;
               double sum = 0.0;
               double newmax = 0;
               var p = this.featureVector[i];
               //Normalize the entries
               for (int j = 0; j < this.Clusters.Count; j++)
               {
                   max = U[i, j] > max ? U[i, j] : max;
                   min = U[i, j] < min ? U[i, j] : min;
               }
               //Sets the values to the normalized values between 0 and 1
               for (int j = 0; j < this.Clusters.Count; j++)
               {
                   U[i, j] = (U[i, j] - min) / (max - min);
                   sum += U[i, j];
               }
               //Makes it so that the sum of all values is 1 
               for (int j = 0; j < this.Clusters.Count; j++)
               {
                   U[i, j] = U[i, j] / sum;
                   if (double.IsNaN(U[i, j]))
                   {
                       ///Console.WriteLine("NAN value: point({0}) cluster({1}) sum {2} newmax {3}", i, j, sum, newmax);
                       U[i, j] = 0.0;
                   }
                   //Console.WriteLine("ClusterIndex: point({0}) cluster({1}) min {2} max {3} value {4} p.ClusterIndex {5}", i, j, min, max, U[i, j], p.ClusterIndex);
                   newmax = U[i, j] > newmax ? U[i, j] : newmax;
               }
             }
        }

        /// <summary>
        /// Perform one step of the algorithm
        /// </summary>
        public void Step()
        {
            for (int i = 0; i < Clusters.Count; i++)
            {
                for (int k = 0; k < featureVector.Count; k++)
                {
                    double top = Math.Pow(CalculateEuclideanDistance(featureVector[k], Clusters[i].Length), 2.0);

                    if (top < 1.0) 
                        top = Eps;

                    // sumTerms is the sum of distances from this data point to all clusters.
                    double sumTerms = 0.0;

                    for (int j = 0; j < Clusters.Count; j++)
                    {
                        sumTerms += Math.Pow(CalculateEuclideanDistance(featureVector[k], Clusters[j].Length), 2.0);
                    }

                    U[k, i] = (double)(1.0 / Math.Pow(top / sumTerms, (2 / (this.Fuzzyness - 1)))); 
                }
            }

            this.RecalculateClusterMembershipValues();
        }

        /// <summary>
        /// Calculates Euclidean Distance distance between a point and a cluster centroid
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="c">Centroid</param>
        /// <returns>Calculated distance</returns>
        private double CalculateEuclideanDistance(double x, double v) 
        {
            return Math.Sqrt(Math.Pow(x - v, 2.0)); 
        }

        /// <summary>
        /// Calculate the objective function
        /// </summary>
        /// <returns>The objective function as double value</returns>
        public double CalculateObjectiveFunction()
        {
            double Jk = 0.0;

            for (int k = 0; k < this.featureVector.Count; k++)
            {
                for (int i = 0; i < this.Clusters.Count; i++)
                {
                    Jk += Math.Pow(U[k, i], this.Fuzzyness) * Math.Pow(this.CalculateEuclideanDistance(featureVector[k], Clusters[i].Length), 2);
                }
            }
            return Jk;
        }

        /// <summary>
        /// Calculates the centroids of the clusters 
        /// </summary>
        public void CalculateClusterCentroids()
        {
            //Console.WriteLine("Cluster Centroid calculation:");
            for (int i = 0; i < this.Clusters.Count; i++)
            {
                ClusterCentroid c = this.Clusters[i];
                double l = 0.0;
                double numerator = 0.0; //分子項
                double denominator = 0.0; //分母項

                for (int k = 0; k < this.featureVector.Count; k++)
                {
                    l = Math.Pow(U[k, i], this.Fuzzyness);

                    numerator += l * featureVector[k];
                    denominator += l;
                }

                c.Length = numerator / denominator;
             }
        }
    }
}
