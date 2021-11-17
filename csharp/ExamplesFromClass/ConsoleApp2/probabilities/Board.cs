using System.Collections.Generic;

namespace ConsoleApp2.probabilities
{
    /// <summary>Class <c>Board</c> models the complete board game, and its updated status after the stateful navigation.</summary>
    public class Board {
        public Door[][] transition_from_states;
        public List<bool> state_navigation; // better: BitArray
        public ulong total_states;
        public ulong total_doors;
        public ulong initial_vertex;
        public ulong final_vertex;

        /// <summary>
        /// Initiali
        /// </summary>
        /// <param name="n">Number of total states within the game (default = 4)</param>
        /// <param name="doors">Number of total doors per state (default = 6)</param>
        public Board(ulong n = 4, ulong doors = 6) {
            total_states = n;
            total_doors = doors;
            initial_vertex = 0;
            final_vertex = (n == 0 ? 0 : n - 1);
            transition_from_states = new Door[n][];
            state_navigation = new List<bool>();
            for (ulong i = 0; i<n-1; i++) {
                transition_from_states[i] = new Door[doors];
                for (ulong j = 0; j < doors; j++) {
                    transition_from_states[i][j] = new Door();
                }
            }
            transition_from_states[n - 1] = new Door[0];
        }
    }
}
