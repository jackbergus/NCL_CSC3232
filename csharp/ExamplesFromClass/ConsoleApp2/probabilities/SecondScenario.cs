using System;

namespace ConsoleApp2.probabilities
{

    public class OnWrongDoBacktrack : SimpleNavigationFunction
    {
        /// <summary>
        /// When a wrong door is choosen, navigates towards a state while remembering that the state was incorrect
        /// </summary>
        /// <param name="currState">Current state from which start the navigation</param>
        /// <param name="b">Board containing the stateful information</param>
        /// <param name="d">Door that was currently choosen</param>
        /// <returns></returns>
        public bool onWrongDoorDo(ref ulong currState, ref Board b, ref Door d)
        {
            Console.WriteLine("   Moving towards state #{0}", d.reachable_state);
            currState = (ulong)d.reachable_state;
            b.state_navigation.Add(false);// Add a wrong state to the navigation
            return true;                  // Continue playing the game without interrupting the game
        }

        /// <summary>
        /// Backtracks the game play to the initial state of the game
        /// </summary>
        /// <param name="currState">Current state from which perform the backtrack</param>
        /// <param name="b">Boad containing the initial state information from which re-start the game</param>
        public void onWrongStateDo(ref ulong currState, ref Board b)
        {
            currState = b.initial_vertex;
            b.state_navigation.Clear();
        }
    }

    public class SecondScenario : BasicScenarios {
        public SecondScenario(ulong n = 4, ulong doors = 6) : base(n, doors) { }

        public override Board initScenario()
        {
            Board board = new Board(nStates, nDoors);
            long i = 0, M = board.transition_from_states.Length;
            foreach (Door[] adj in board.transition_from_states)
            {
                long j = 0;
                long N = (long)adj.Length;
                foreach (Door refD in adj)
                {
                    refD.locked = false;
                    refD.reachable_state = (i == (M-1) ? -1 : i+1);
                    if (j == (N - 1))
                    {
                        refD.wrong_door = false;
                    }
                    j++;
                }
                i++;
            }
            return board;
        }

        public override void initSimpleNavigationFunction()
        {
            if (f == null)
                f = new OnWrongDoBacktrack();
        }
    }
}
