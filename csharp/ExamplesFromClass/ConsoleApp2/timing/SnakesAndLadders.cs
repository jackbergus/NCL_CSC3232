using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.timing
{
    class SnakesAndLadders
    {
        DiscreteTimeMarkovChain dtmc;

        public SnakesAndLadders(int NCells, HashSet<utils.Pair<UInt64, UInt64>> snakes, HashSet<utils.Pair<UInt64, UInt64>> ladders) {
            // Using a dense matrix
            Matrix<double> T = new DenseMatrix(NCells+1, NCells+1);

            // Setting the standard transition, without snakes and ladders
            for (int i = 1; i<NCells+1; i++)
            {
                for (int j = 0; j<6 && i+j<NCells+1; j++)
                    T[i - 1, i + j] = 1.0 / 6.0;
            }

            // Merging snake and ladders, as they only differ on the order of the first and second element:
            // * Each ladder (i,j) is i<j
            // * Each snake (i,j) is j<i
            HashSet<utils.Pair<UInt64, UInt64>> totalMoves = new HashSet<utils.Pair<UInt64, UInt64>>();
            totalMoves.UnionWith(snakes);
            totalMoves.UnionWith(ladders);
            foreach (var cp in totalMoves)
            {
                // determine the cells x arriving at the trick (snake or ladder)
                var ls = T.Column((int)cp.key).EnumerateIndexed().Where((x) => x.Item2 > 0.0).Select(x => x.Item1).ToList();
                // no cell shall arrive at the beginning of the trick
                for (int i = 0; i < NCells + 1; i++)
                {
                    T[i, (int)cp.key] = 0.0;
                }
                // these cells x will directly transit towards the end of the trick (snake or ladder)
                foreach (var x in ls)
                    T[x, (int)cp.value] += 1.0 / 6.0;
            }
            // If the dice returns {6} and I am at position 95, I shall end at 100.
            T[95, 100] += 1.0 / 6.0;
            // If the dice returns {5,6} and I am at position 96, I shall end at 100.
            T[96, 100] += 2.0 / 6.0;
            // If the dice returns {4,5,6} and I am at position 97, I shall end at 100.
            T[97, 100] += 3.0 / 6.0;
            // If the dice returns {3,4,5,6} and I am at position 98, I shall end at 100.
            T[98, 100] += 4.0 / 6.0;
            // If the dice returns {2,3,4,5,6} and I am at position 99, I shall end at 100.
            T[99, 100] += 5.0 / 6.0;
            foreach (var cp in snakes)
            {
                T[(int)cp.key, NCells] = 0;
                T[(int)cp.value, NCells] = 0;
            }

            dtmc = new DiscreteTimeMarkovChain(NCells + 1, T, new HashSet<int>(new int[] { 100 }), new HashSet<int>(new int[] { 0 }));
        }

        /// <summary>
        /// Computes the average hitting time for reaching one of the two accepting states from the initial state
        /// </summary>
        /// <returns></returns>
        public double averageHittingTime()
        {
            return dtmc.computeAverageHittingTimes()[0];
        }

        public String plotProbabilityDensityFunction()
        {
            return dtmc.printProbabilityDensityFunction(dtmc.probabilityDensityFunction(100));
        }

    }
}
