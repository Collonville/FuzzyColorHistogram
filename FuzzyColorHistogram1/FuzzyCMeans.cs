﻿using System;
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
        private ClusterCentroid[] Clusters;

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
            Clusters = new ClusterCentroid[numCluster];
            this.numCluster = numCluster;
            featureVector = points;

            U = new double[this.featureVector.Count, numCluster];
            this.Fuzzyness = fuzzy;

            //Create random points to use a the cluster centroids
            /*
            Random random = new Random();
            for (int i = 0; i < numCluster; i++)
            {
                int randomNumber1 = random.Next(featureVector.Count);

                Clusters.Add(new ClusterCentroid(randomNumber1, featureVector[randomNumber1]));
            }*/

            InitClusterUsingKPlusPlus();

            InitU();

            this.RecalculateClusterMembershipValues();
        }

        private void InitClusterUsingKPlusPlus()
        {
            Random random = new Random();

            int numPoints = featureVector.Count;

            bool[] check = new bool[numPoints];
            double[] minDistSquared = new double[numPoints];
            int firstPoint = random.Next(numPoints);

            int clusterIndex = 0;

            Clusters[clusterIndex] = new ClusterCentroid(firstPoint, featureVector[firstPoint]);
            check[firstPoint] = true;

            // Initialize the elements. 
            for (int i = 0; i < numPoints; i++)
            {
                if (i != firstPoint)
                {
                    double d = Math.Abs(Clusters[clusterIndex].X - featureVector[i]);
                    minDistSquared[i] = d * d;
                }
            }

            int initedCluster = 1;
            while (initedCluster < numCluster)
            {
                double distSqSum = 0.0;
                for (int i = 0; i < numPoints; i++)
                {
                    if (!check[i])
                    {
                        distSqSum += minDistSquared[i];
                    }
                }

                double r = random.NextDouble() * distSqSum;
                

                // The index of the next point to be added to the resultSet.
                int nextPointIndex = -1;
                double sum = 0.0;

                // Sum through the squared min distances again, stopping when
                // sum >= r.
                for(int i = 0; i < numPoints; i++)
                {
                    if (!check[i]) 
                    {
                        sum += minDistSquared[i];
                        if(sum >= r)
                        {
                            nextPointIndex = i;
                            break;
                        }
                    }
                }

                if (nextPointIndex == -1)
                {
                    for (int i = numPoints - 1; i >= 0; i--)
                    {
                        if (!check[i])
                        {
                            nextPointIndex = i;
                            break;
                        }
                    }
                }

                if (nextPointIndex >= 0)
                {
                    clusterIndex++;
                    Clusters[clusterIndex] = new ClusterCentroid(nextPointIndex, featureVector[nextPointIndex]);
                    check[nextPointIndex] = true;

                    if (initedCluster < numCluster)
                    {
                        for (int i = 0; i < numPoints; i++)
                        {
                            if (!check[i])
                            {
                                double d = Math.Abs(Clusters[clusterIndex].X - featureVector[i]);
                                double d2 = d * d;

                                if (d2 < minDistSquared[i])
                                {
                                    minDistSquared[i] = d2;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("None Found,Break from the while loop to prevent,an infinite loop");
                    break;
                }

                initedCluster = 0;
                //初期値が決まっていないクラスタの個数を数える
                for (int i = 0; i < Clusters.Length; i++)
                {
                    if (Clusters[i] != null)
                        initedCluster++;
                }
            }
        }

        private void InitU()
        {
            var rd = new Random();

            for (var x = 0; x < featureVector.Count; x++)
            {
                var maximum = 1.0;
                const double minimum = 0;
                for (var c = 0; c < numCluster; c++)
                {
                    if (c == numCluster - 1)
                    {
                        U[x, c] = maximum;
                    }
                    else
                    {
                        U[x, c] = rd.NextDouble() * (maximum - minimum) + minimum;
                        maximum -= U[x, c];
                    }
                }
            }

            /*
            for (var x = 0; x < featureVector.Count; x++)
            {
                double sum = 0.0;
                for (var c = 0; c < numCluster; c++)
                {
                    sum += U[x, c];
                }
                Console.WriteLine(sum);
            }*/
        }

        public void run()
        {
            int k = 0;
            J = CalculateObjectiveFunction();

            do
            {
                CalculateClusterCentroids();
                Step();
                double Jnew = CalculateObjectiveFunction();
                double diff = Math.Abs(J - Jnew);

                J = Jnew;
          
                Console.WriteLine("Run method i={0} accuracy = {1} delta={2}", k, J, diff);

                if (diff < Eps) 
                    break;

                k++;
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
               for (int j = 0; j < this.Clusters.Length; j++)
               {
                   max = U[i, j] > max ? U[i, j] : max;
                   min = U[i, j] < min ? U[i, j] : min;
               }
               //Sets the values to the normalized values between 0 and 1
               for (int j = 0; j < this.Clusters.Length; j++)
               {
                   U[i, j] = (U[i, j] - min) / (max - min);
                   sum += U[i, j];
               }
               //Makes it so that the sum of all values is 1 
               for (int j = 0; j < this.Clusters.Length; j++)
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
            double p = 2.0 / (this.Fuzzyness - 1.0);

            for (int i = 0; i < Clusters.Length; i++)
            {
                for (int k = 0; k < featureVector.Count; k++)
                {
                    double top = EuclideanDistance(featureVector[k], Clusters[i].Length);

                    // sumTerms is the sum of distances from this data point to all clusters.
                    double sumTerms = 0.0;
                    for (int j = 0; j < Clusters.Length; j++)
                    {
                        double val = top / EuclideanDistance(featureVector[k], Clusters[j].Length);
                        sumTerms += Math.Pow(val, p);
                    }

                    U[k, i] = 1.0 / sumTerms;
                }
            }

            this.RecalculateClusterMembershipValues();
        }

        private double EuclideanDistance(double x, double v) 
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
                for (int i = 0; i < this.Clusters.Length; i++)
                {
                    Jk += Math.Pow(U[k, i], this.Fuzzyness) * Math.Pow(this.EuclideanDistance(featureVector[k], Clusters[i].Length), 2);
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
            for (int i = 0; i < this.Clusters.Length; i++)
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
