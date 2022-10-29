using System;
using System.Diagnostics;

namespace ConsoleApp2.probabilities
{
    class Probabilities {
        public static void main()
        {

            // 1. Running the first scenario from the lecture #1 
            if (true) {
                FirstScenario fs = new FirstScenario();
                //    a) computing the number of total turns from the best case scenario
                ulong test = fs.runBestCaseScenario();
                Debug.Assert(test == 3, "Wrong number of expected cases (got " + ((UInt64)test).ToString() + ", expected 3)");

                //    b) computing the number of total turns for the worst case scenario
                test = fs.runWorstCaseScenario();
                Debug.Assert(test == 18, "Wrong number of expected cases (got " + ((UInt64)test).ToString() + ", expected 18)");
            }

            // 2. Running the second scenario from lecture #1
            if (false) {
                SecondScenario sn = new SecondScenario();
                //    a) computing the number of total turns from the best case scenario. It is always the same across all the possible scenarios
                ulong test = sn.runBestCaseScenario();
                Debug.Assert(test == 3, "Wrong number of expected cases (got " + ((UInt64)test).ToString() + ", expected 3)");

                //    b) computing the number of total turns for the worst case scenario
                //    This scenario, with no strategy, is expected to diverge (i.e., never return a value)
                // sn.runWorstCaseScenario(); 
            }

            // 3. Running the third scenario from lecture #1, which is running the previous one with a given strategy
            if (true) {
                double sum = 0;
                double max = 5000;
                long max_val = -1;
                ulong min_val = ulong.MaxValue;
                for (int i = 0; i < (uint)max; i++)
                {
                    ///Console.WriteLine("Seed: {0}", i);
                    ThirdScenario ts = new ThirdScenario(i);
                    ulong val = ts.testRandomCaseScenario(false);
                    min_val = Math.Min(val, min_val);
                    max_val = Math.Max((long)val, max_val);
                    sum += (double)val;
                }

                Console.WriteLine("MIN = {0}", min_val);
                Console.WriteLine("MAX = {0}", max_val);
                Console.WriteLine("Average = {0}", (sum / max));
            }

            // 4/5. Playing with both the fourth and the fifth scenario
            if (false) {
                double max = 100.0;
                double maxVal = -1;
                double minVal = Double.MaxValue;
                double avgVal = 0.0;
                for (int i = 0; i < (int)max; i++)
                {
                    FourthAndFifthScenario ff = new FourthAndFifthScenario(6, 3, i, 1.0);
                    double pg = (double)ff.playGame(true);
                    Console.WriteLine("pg[true] = {0}", pg);
                    maxVal = Math.Max(maxVal, pg);
                    minVal = Math.Min(minVal, pg);
                    avgVal += pg;
                }
                avgVal = avgVal / max;
                Console.WriteLine("Min = {0}", minVal);
                Console.WriteLine("Max = {0}", maxVal);
                Console.WriteLine("Avg = {0}", avgVal);
            }

            Console.WriteLine();

            if (false) {
                double max = 100.0;
                double maxVal = -1;
                double minVal = Double.MaxValue;
                double avgVal = 0.0;
                for (int i = 0; i < (int)max; i++)
                {
                    FourthAndFifthScenario ff = new FourthAndFifthScenario(6, 3, i, 1.0);
                    double pg = (double)ff.playGame(false);
                    Console.WriteLine("pg[false] = {0}", pg);
                    maxVal = Math.Max(maxVal, pg);
                    minVal = Math.Min(minVal, pg);
                    avgVal += pg;
                }
                avgVal = avgVal / max;
                Console.WriteLine("Min = {0}", minVal);
                Console.WriteLine("Max = {0}", maxVal);
                Console.WriteLine("Avg = {0}", avgVal);
            }
        }
    }
}
