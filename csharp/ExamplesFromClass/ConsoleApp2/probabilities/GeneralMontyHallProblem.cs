using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp2.probabilities
{
    class GeneralMontyHallProblem {

        Door[] stateDoors;
        int correctDoor;
        int nDoors;
        int p;
        Random generator;
        utils.UniformRandom ur;
        utils.UniformRandom uRem;

        /// <summary>
        /// Initializing a state, by setting up an instance of the general MontyHallProblem
        /// </summary>
        /// <param name="correctDoor">offset of the correct door</param>
        /// <param name="nextReachableState">next Reachable State from the current state</param>
        /// <param name="currentStateSeed">Seed associated to the random number generator for the current state</param>
        /// <param name="p">Number of doors to be showed while running the MontyHall problem</param>
        /// <param name="nDoors">Number of doors associated to the state </param>
        public GeneralMontyHallProblem(int nextReachableState, int currentStateSeed, int p = 4, int nDoors = 6, bool debug = false) {
            generator = new Random(currentStateSeed);
            ur = new utils.UniformRandom(0, nDoors - 1);
            uRem = new utils.UniformRandom(0, p - 1);
            correctDoor = ur.nextInt(ref generator);
            if (debug) Console.WriteLine("Setting Correct Door = {0}", correctDoor);

            Debug.Assert(p < nDoors - 1, "The number of the showed rooms should be less than the number of the doors -1!");
            stateDoors = Enumerable.Range(0, nDoors).Select(i => new Door()).ToArray();
            long j = 0;
            Debug.Assert(correctDoor < nDoors);
            foreach (Door refD in stateDoors)
            {
                refD.locked = false;
                refD.reachable_state = nextReachableState;
                if (j == correctDoor)
                {
                    refD.wrong_door = false;
                }
                j++;
            }

            this.nDoors = nDoors;
            this.p = p;
        }

        /// <summary>
        /// Checks whether i is the correct door
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool isCorrectDoor(int i)  {
            return i == correctDoor;
        }

        /// <summary>
        /// Change the correct door associated to the current state
        /// </summary>
        public void changeCorrectDoor(bool debug = false)
        {
            int nextDoor = 0;
            do
            {
                nextDoor = ur.nextInt(ref generator);
            } while (nextDoor == correctDoor);
            stateDoors[correctDoor].wrong_door = true;
            stateDoors[nextDoor].wrong_door = true;
            if (debug) Console.WriteLine("Changing Correct Door = {0}", nextDoor);
            correctDoor = nextDoor;
        }

        /// <summary>
        /// Plays the game and picks a door
        /// </summary>
        /// <param name="runMontyPythonGame"></param>
        /// <returns></returns>
        public int pickADoor(bool runMontyHallGame = true) {

            // The player picks a door
            int pickDoor = ur.nextInt(ref generator);

            if (runMontyHallGame) {
                // Getting other p wrong doors, that should be different from the previous choice by the user
                HashSet<int> choices = new HashSet<int>();
		do {
		    var nextDoor = ur.nextInt(ref generator);
		    if ((nextDoor != pickDoor) && (nextDoor != correctDoor))
		      choices.Add(nextDoor);
		} while (choices.Count < p);

                // Retrieveing the remaining N-p-1 doors, and choosing one of it
                int nextChoiceOffset = uRem.nextInt(ref generator);
                pickDoor = -1;
                for (int i = 0; (i < nDoors) && (nextChoiceOffset >= 0); i++)
                {
                    if (((!choices.Contains(i)) && (pickDoor != correctDoor))) {
                        pickDoor = i;
                        nextChoiceOffset--;
                    }
                }
                choices.Clear();
                Debug.Assert(pickDoor >= 0);
            }

            return pickDoor;
        }
    }
}
