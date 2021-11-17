using System;

namespace ConsoleApp2.probabilities
{


    public interface SimpleNavigationFunction {
        bool onWrongDoorDo(ref ulong x, ref Board b, ref Door d);
        void onWrongStateDo(ref ulong currState, ref Board b);
    }



    class SimpleNavigation
    {
        /// <summary>
        /// Performs the game-play navigation towards the next step. It uses a SimpleNavigationFunction to determine the best policy to adopt
        /// </summary>
        /// <param name="board">Board on which we are playing</param>
        /// <param name="curr_state">Current state within the board</param>
        /// <param name="d">Board currently choosen from the game</param>
        /// <param name="door_id">Id associated to the current door</param>
        /// <param name="attempts">Counting the number of attempts</param>
        /// <param name="set_choosen">Whether the same door was choosen in a previous iteration of the game</param>
        /// <param name="f">Handling function for </param>
        /// <returns>Returns true if you need to kill the iteration over the doors to visit, and false otherwise</returns>
        public static bool current_door_step(ref Board board,
                                             ref ulong curr_state,
                                             long door_id,
                                             ref ulong attempts,
                                             SimpleNavigationFunction f) {
            ref Door d = ref board.transition_from_states[curr_state][door_id];

            attempts++;
            if (d.locked)
            {
                Console.WriteLine("   Door is locked!");
            }
            else if (d.wrong_door)
            {
                Console.WriteLine("   Wrong Door!");
                return f.onWrongDoorDo(ref curr_state, ref board, ref d);
            }
            else
            {
                Console.WriteLine("   Moving towards state #{0}", d.reachable_state);
                curr_state = (ulong)d.reachable_state;
                board.state_navigation.Add(true);
                return true; // Killing the iteration!
            }
            return false; // Do not kill the iteration
        }


        public static ulong navigate(ref Board board, 
                                    bool worst_case_scenario,
                                    SimpleNavigationFunction f) {
            ulong attempts = 0;
            ulong curr_state = board.initial_vertex;
            if (worst_case_scenario)
                Console.WriteLine("Simulating the Worst Case Scenario");
            else
                Console.WriteLine("Simulating the Best Case Scenario");
            while (curr_state != board.final_vertex)
            {
                Console.WriteLine(" * Current State = {0}", curr_state);
                int doorNo = board.transition_from_states[curr_state].Length;
                if (worst_case_scenario)
                {
                    for (int doorId = 0; doorId < doorNo; doorId++)
                    {
                        if (current_door_step(ref board, ref curr_state, doorId, ref attempts, f)) break;
                    }
                }
                else
                {
                    for (int doorId = doorNo-1;  doorId >= 0; doorId--) {
                        if (current_door_step(ref board, ref curr_state, doorId, ref attempts, f)) break;
                    }
                }
                if (curr_state == board.final_vertex) {
                    bool is_ok = true;
                    foreach (bool v in board.state_navigation) {
                        if (!v) {
                            is_ok = false;
                            break;
                        }
                    }
                    if (!is_ok)
                        f.onWrongStateDo(ref curr_state, ref board);
                }
            }
            Console.WriteLine("Total attempts = {0}", attempts);
            return attempts;
        }
    }



}
