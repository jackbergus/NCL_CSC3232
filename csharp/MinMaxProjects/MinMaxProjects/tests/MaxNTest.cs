using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinMaxLibrary.algorithms;
using MinMaxLibrary.utils;

namespace MinMaxProjects.tests
{
    public class MaxNTest
    {

        private static new MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player genPlayerWithScore(HashSet<uint> actionSet, double val = 0.0, bool type = true)
        {
            return new MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player(actionSet, new TreeBasedPlayerConf(val), type);
        }

        private static new List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player> genPlayerListWithScore(double val1, double val2, double val3)
        {
            List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player> ls = new List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player>();
            ls.Add(genPlayerWithScore(null, val1));
            ls.Add(genPlayerWithScore(null, val2));
            ls.Add(genPlayerWithScore(null, val3, false));
            return ls;
        }

        public static void test()
        {
            // Each player might perform only two actions
            HashSet<uint> actionSet = new HashSet<uint>();
            uint isLeftBranch = 0;
            uint isRightBranch = isLeftBranch + 1;
            actionSet.Add(isLeftBranch);   // action on the left branch
            actionSet.Add(isRightBranch);  // action on the right branch

            // I need to create three players
            List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player> playerlist 
                = new List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player>();
            for (int i = 0; i<3; i++)
                playerlist.Add(genPlayerWithScore(actionSet, 0.0, i < 2));

            // Generating a multidimensional array, each containing the actions provided in the i-th turn by the (i+1)-th player
            List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player>[,,] scoreForEachTerminalNode = 
                new List<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.Player>[2,2,2];
            
            // Action Left for A
            scoreForEachTerminalNode[0, 0, 0] = genPlayerListWithScore(3, 3, 3);
            scoreForEachTerminalNode[0, 0, 1] = genPlayerListWithScore(2, 2, 5);
            scoreForEachTerminalNode[0, 1, 0] = genPlayerListWithScore(2, 5, 2);
            scoreForEachTerminalNode[0, 1, 1] = genPlayerListWithScore(0, 4, 4);

            // Action Right for A
            scoreForEachTerminalNode[1, 0, 0] = genPlayerListWithScore(5, 2, 2);
            scoreForEachTerminalNode[1, 0, 1] = genPlayerListWithScore(4, 0, 4);
            scoreForEachTerminalNode[1, 1, 0] = genPlayerListWithScore(4, 4, 0);
            scoreForEachTerminalNode[1, 1, 1] = genPlayerListWithScore(1, 1, 1);

            /// Definition of a verbose transition function.
            /// In particular, I'm setting up a tree from scratch!
            /// For real games, you should only provide the update for each player
            /// and not setting like in here the values associated to the last values.
            /// This is just for demonstration and debugging purposes, and for showing
            /// how to reproduce the results that you were seeing in class.
            Func<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState,
                               uint, Optional<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState>> f = (conff, act) =>
                               {
                                   int Nact = conff.gameConfiguration.actionsPerformed.Count;
                                   if (Nact <2) {
                                       Debug.Assert(conff.playerIdTurn <= Nact); // Braces and belts programming: the first player should be playing first
                                       Debug.Assert(act < 2); //                             : the action should be one of the two allowed!
                                                                              // Players #1 and #2
                                                                              // Next state, after action act is performed
                                       var result = new MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState(conff, act, (conff.playerIdTurn +1) % 3);
                                       // Recording the action in the collection of all the actions up to this point
                                       result.gameConfiguration = conff.gameConfiguration.appendAction(act);
                                       // Action performed by the parent, leading to the current child state
                                       result.parentAction = act;
                                       // Resulting child configuration
                                       return result;
                                   } else if (Nact == 2)
                                   {
                                       Debug.Assert(conff.playerIdTurn == Nact); // Braces and belts programming: the first player should be playing first
                                       Debug.Assert(actionSet.Contains(act)); //                             : the action should be one of the two allowed!
                                                                              // Player #3


                                       var result = new MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState(conff, act, -1);
                                       result.gameConfiguration = conff.gameConfiguration.appendAction(act);

                                      
                                       // reaching the leaves: setting up the opponent values.
                                       // This corresponds to the long-winded if-then-else statement in MinMaxABTests!
                                       // By replacing the strings with numbers, we can access everything via indices
                                       // in a far more efficient way.
                                       result.players = scoreForEachTerminalNode[conff.gameConfiguration.actionsPerformed[0],
                                                                                    conff.gameConfiguration.actionsPerformed[1],
                                                                                     act];


                                       result.parentAction = act;
                                       return result;

                                   }

                                   // Otherwise, terminate the tree
                                   return new Optional<MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState>();
                               };

            /// Setting an instance of the algorithm
            var cgs = new MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>(playerlist, f);

            /// Setting the initial configuration of the world, in a similar way from the former game
            MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState treeRoot = new MaxNAlgorithm<uint, TreeBasedGameConfiguration<uint>, TreeBasedPlayerConf>.CurrentGameState();
            treeRoot.players = playerlist;
            treeRoot.gameConfiguration = new MinMaxProjects.tests.TreeBasedGameConfiguration<uint>();
            treeRoot.playerIdTurn = 0;

            /// Running the complete algorithm, with no alpha beta pruning (sob!)
            var tree = cgs.fitModel(treeRoot);
            tree.Print((x) => x.ToString());
        }

    }
}
