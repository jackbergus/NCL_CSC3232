using System;
using System.Linq;

namespace ConsoleApp2.probabilities
{
    class FourthAndFifthScenario {

        int nDoors;
        int nStates;
        GeneralMontyHallProblem[] states;

        public FourthAndFifthScenario(int nDoors, int nStates, int seed, double p = 0.8)
        {
            this.nDoors = nDoors;
            this.nStates = nStates;
            int pInt = Math.Min((int)Math.Round(p * ((double)nDoors-2)), nDoors-2);
            states = Enumerable.Range(0, nStates).Select(i => new GeneralMontyHallProblem(i+1, seed+i, pInt, nDoors)).ToArray();
        }

        public ulong playGame(bool runMontyHallGame = true, bool debug = false)
        {
            ulong countAttempts = 0;
            int[] attempt = new int[nStates];
            bool correctAttempt = true;

            do
            {
                correctAttempt = true;
                for (int i = 0; i < nStates; i++) {
                    attempt[i] = states[i].pickADoor(runMontyHallGame);
                    correctAttempt = correctAttempt && states[i].isCorrectDoor(attempt[i]);
                    states[i].changeCorrectDoor();// Saving up the iteration time!
                }
                if (debug) Console.WriteLine("[{0}]\n\n", string.Join(", ", attempt.Select(i => i.ToString())));
                countAttempts++;
            } while (!correctAttempt);

            return countAttempts;
        }
    }
}
