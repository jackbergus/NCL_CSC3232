using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace ConsoleApp2.timing
{
    class DiscreteTimeMarkovChain
    {
        Matrix<double> T;
        int N;
        HashSet<int> finalStates;
        HashSet<int> initialStates;

        /// <summary>
        /// Implements a general DTMC
        /// </summary>
        /// <param name="N">Number of total states</param>
        /// <param name="sparseMatrix">Non-zero cells of the sparse matrix</param>
        /// <param name="finalStates">Final states for the DTMC</param>
        /// <param name="initialStates">Initial states for the DTMC</param>
        public DiscreteTimeMarkovChain(int N, List<Tuple<int, int, double>> sparseMatrix, HashSet<int> finalStates, HashSet<int> initialStates) {
            T = SparseMatrix.OfIndexed(N, N, sparseMatrix);
            this.N = N;
            Debug.Assert(T.RowCount == N);
            Debug.Assert(T.ColumnCount == N);
            this.finalStates = finalStates;
            this.initialStates = initialStates;
        }

        public DiscreteTimeMarkovChain(int N, Matrix<double> T, HashSet<int> finalStates, HashSet<int> initialStates)
        {
            this.T = T;
            this.N = N;
            Debug.Assert(T.RowCount == N);
            Debug.Assert(T.ColumnCount == N);
            this.finalStates = finalStates;
            this.initialStates = initialStates;
        }

        /// <summary>
        /// Computes the average hitting time for all the final states
        /// </summary>
        /// <returns>A dictionary mapping the final state id to its associated average hitting time</returns>
        public Dictionary<int, double> computeAverageHittingTimes() {
            Dictionary<int, double> result = new Dictionary<int, double>();
            Vector<double> b = SparseVector.Create(N, (i) => finalStates.Contains(i) ? 0 : (-1));
            Matrix<double> A = SparseMatrix.Create(N, N, (i, j) =>
            {
                if (i == j)
                {
                    if (finalStates.Contains(i))
                        return 1.0;
                    else
                        return -1.0;
                }
                else if (finalStates.Contains(i))
                {
                    return 0.0;
                }
                else
                    return T[i, j];
            });
            var x = A.Solve(b);
            foreach (var start in initialStates)
                result.Add(start, x[start]);
            return result;
        }

        /// <summary>
        /// Plots the probability Density Function for the execution time, starting from the first step (first element of the list) towards the last step where we cumulated nearly
        /// the total probability values.
        /// </summary>
        /// <param name="maxIterations">Maximum number of iterations after which check whether the probability values are neglegible</param>
        /// <returns>Each i-th element of the list represents the probability density function after i computational steps from the initial state,
        ///          and each element represents a dictionary representing the probability density function of a given final state.</returns>
        public List<Dictionary<int, double>> probabilityDensityFunction(int maxIterations, double maxCumulative = 0.99999, double minProbability = 1e-16) {
            List<Dictionary<int, double>> result = new List<Dictionary<int, double>>();
            Vector<double> v = SparseVector.Create(N, (i) => initialStates.Contains(i) ? 1 : 0);
            Dictionary<int, double> cumulativeDistributionFunction = new Dictionary<int, double>();
            foreach (var x in finalStates)
                cumulativeDistributionFunction.Add(x, 0.0);
            int n = 1;

            /// The iteration will halt when we reach a cumulative probability value near to 1 (maxCumulative)
            while (cumulativeDistributionFunction.All((cp) => cp.Value < maxCumulative) &&
                    ((n<maxIterations) || finalStates.Select(i => Math.Abs(v[i])).All(w => w >= minProbability))) {
                v = T.LeftMultiply(v);
                n++;
                Dictionary<int, double> d = new Dictionary<int, double>();
                foreach (var x in finalStates) {
                    cumulativeDistributionFunction[x] += v[x];
                    d.Add(x, v[x]);
                }
                result.Add(d);
            }
            return result;
        }

        /// <summary>
        /// Represents the probability density function as a String
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public String printProbabilityDensityFunction(List<Dictionary<int, double>> ls) {
            return printProbabilityDensityFunction(ls, finalStates);
        }

        private static String printProbabilityDensityFunction(List<Dictionary<int, double>> ls, HashSet<int> finalStates) {
            StringBuilder sb = new StringBuilder();
            var X = finalStates.ToList();
            X.Sort();

            sb.Append("Final States = {");
            sb.Append(string.Join(", ", X.Select(i => i.ToString())));
            sb.Append("}\n\n");

            for (int i = 0; i < ls.Count; i++) {
                sb.Append(i.ToString());
                sb.Append(" = [");
                var el = ls.ElementAt(i);
                sb.Append(string.Join(", ", X.Select(j => el[j].ToString())));
                sb.Append("]\n");
            }

            return sb.ToString();
        }

    }
}
