using System;

namespace ConsoleApp2
{
    class Program {
        static void Main(string[] args) {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            probabilities.Probabilities.main();
            ///timing.Timing.main();
            ///goap.GOAP.main();

            Console.WriteLine("Press any key (except Enter) to exit");
            Console.ReadKey();
        }
    }
}
