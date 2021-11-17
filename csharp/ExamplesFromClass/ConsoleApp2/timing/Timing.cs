using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.timing
{
    class Timing {
        public static void main() {

            if (false) {
                for (uint i = 1; i < 10; i++) {
                    double iP = ((double)i) / 10.0;
                    for (uint j = 1; j < 10; j++) {
                        double jP = ((double)j) / 10.0;

                        Pong p = new Pong(iP, jP);
                        Console.WriteLine("AverageHittingTime(p={0},q={1}) = {2}", iP, jP, p.averageHittingTime());
                        Console.WriteLine(p.plotProbabilityDensityFunction());
                    }
                }
            }

            {
                HashSet<utils.Pair<UInt64, UInt64>> snakes = new HashSet<utils.Pair<UInt64, UInt64>>();
                HashSet<utils.Pair<UInt64, UInt64>> ladders = new HashSet<utils.Pair<UInt64, UInt64>>();


                {
                    SnakesAndLadders noSnakeNoLadders = new SnakesAndLadders(100, snakes, ladders);
                    Console.WriteLine("AverageHittingTime(nS, nL) = {0}", noSnakeNoLadders.averageHittingTime());
                    Console.WriteLine(noSnakeNoLadders.plotProbabilityDensityFunction());
                }

                snakes.Add(new utils.Pair<UInt64, UInt64>(3, 19));
                snakes.Add(new utils.Pair<UInt64, UInt64>(15, 37));
                snakes.Add(new utils.Pair<UInt64, UInt64>(22, 42));
                snakes.Add(new utils.Pair<UInt64, UInt64>(25, 64));
                snakes.Add(new utils.Pair<UInt64, UInt64>(41, 73));
                snakes.Add(new utils.Pair<UInt64, UInt64>(53, 74));
                snakes.Add(new utils.Pair<UInt64, UInt64>(63, 86));
                snakes.Add(new utils.Pair<UInt64, UInt64>(76, 91));
                snakes.Add(new utils.Pair<UInt64, UInt64>(84, 98));


                {
                    SnakesAndLadders noSnakeNoLadders = new SnakesAndLadders(100, snakes, ladders);
                    Console.WriteLine("AverageHittingTime(S, nL) = {0}", noSnakeNoLadders.averageHittingTime());
                    Console.WriteLine(noSnakeNoLadders.plotProbabilityDensityFunction());
                }


                ladders.Add(new utils.Pair<UInt64, UInt64>(11, 7));
                ladders.Add(new utils.Pair<UInt64, UInt64>(18, 13));
                ladders.Add(new utils.Pair<UInt64, UInt64>(28, 12));
                ladders.Add(new utils.Pair<UInt64, UInt64>(36, 34));
                ladders.Add(new utils.Pair<UInt64, UInt64>(77, 16));
                ladders.Add(new utils.Pair<UInt64, UInt64>(47, 26));
                ladders.Add(new utils.Pair<UInt64, UInt64>(83, 39));
                ladders.Add(new utils.Pair<UInt64, UInt64>(92, 75));
                ladders.Add(new utils.Pair<UInt64, UInt64>(99, 70));


                {
                    SnakesAndLadders noSnakeNoLadders = new SnakesAndLadders(100, snakes, ladders);
                    Console.WriteLine("AverageHittingTime(S, L) = {0}", noSnakeNoLadders.averageHittingTime());
                    Console.WriteLine(noSnakeNoLadders.plotProbabilityDensityFunction());
                }

                snakes.Clear();
                {
                    SnakesAndLadders noSnakeNoLadders = new SnakesAndLadders(100, snakes, ladders);
                    Console.WriteLine("AverageHittingTime(nS, L) = {0}", noSnakeNoLadders.averageHittingTime());
                    Console.WriteLine(noSnakeNoLadders.plotProbabilityDensityFunction());
                }
            }

        }
    }
}
