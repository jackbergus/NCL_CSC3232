using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.timing
{
    class Pong
    {
        double p;
        double q;
        DiscreteTimeMarkovChain dtmc;

        public Pong(double p, double q)
        {
            this.p = p;
            this.q = q;
            // Sanity check
            Debug.Assert((p <= 1) && (p >= 0));
            Debug.Assert((q <= 1) && (q >= 0));

            // Determing the tuples for the bulk insertion.
            // The non-set triples are assumed to be empty cells in the matrix
            List<Tuple<int, int, double>> ls = new List<Tuple<int, int, double>>();
            ls.Add(new Tuple<int, int, double>(0, 1, 0.5));
            ls.Add(new Tuple<int, int, double>(0, 2, 0.5));
            ls.Add(new Tuple<int, int, double>(1, 2, p));
            ls.Add(new Tuple<int, int, double>(1, 4, 1-p));
            ls.Add(new Tuple<int, int, double>(2, 1, q));
            ls.Add(new Tuple<int, int, double>(2, 3, 1 - q));

            // Setting the final and initial states
            HashSet<int> finalStates = new HashSet<int>(new int[]{3,4});
            HashSet<int> intialStates = new HashSet<int>(new int[] { 0 });

            // Generating the DTMC
            dtmc = new DiscreteTimeMarkovChain(5, ls, finalStates, intialStates);
        }

        /// <summary>
        /// Computes the average hitting time for reaching one of the two accepting states from the initial state
        /// </summary>
        /// <returns></returns>
        public double averageHittingTime()
        {
            return dtmc.computeAverageHittingTimes()[0];
        }

        public String plotProbabilityDensityFunction() {
            return dtmc.printProbabilityDensityFunction(dtmc.probabilityDensityFunction(10));
        }


    }
}
