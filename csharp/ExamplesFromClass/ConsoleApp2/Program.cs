using System;
using ConsoleApp2.goap;

namespace ConsoleApp2
{
    class Program {
        static void Main(string[] args) {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            BasicDungeonGeneration.testing();
            //probabilities.Probabilities.main();
            ///timing.Timing.main();
            ///goap.GOAP.main();

            Console.WriteLine("Press any key (except Enter) to exit");
            Console.ReadKey();
        }
    }
}
