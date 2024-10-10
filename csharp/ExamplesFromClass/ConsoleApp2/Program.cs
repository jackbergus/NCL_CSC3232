using System;
using ConsoleApp2.goap;
using System.Threading;
using ConsoleApp2.utils;

namespace ConsoleApp2
{
    class Program {
        static void Main(string[] args) {

            //Goomba.GoombaExample();
            ConsoleApp2.graphs.ReachabilityProblem<Pair<int,int>>.ReachabilityTest();

            //
            //VariationOnMinMaxRealisticWithRL.main();
            // Console.OutputEncoding = System.Text.Encoding.UTF8;
            // BasicDungeonGeneration.testing();
            //probabilities.Probabilities.main();
            ///timing.Timing.main();
            ///goap.GOAP.main();

            Console.WriteLine("Press any key (except Enter) to exit");
            Console.ReadKey();
        }
    }
}
