using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleApp2.goap.algorithms;

namespace ConsoleApp2.goap
{
    public class VariationOnMinMaxRealisticWithRL
    {

        public static int setGraphRecursively(double enemyHealthBar, double playerHealthBar,
                                              double enemyStrength, double playerStrength,
                                              int eWin, int eLose,
            ref WeightedMultiGraph<string, string> G)
        {
            HashSet<string> actionsPlayer = new HashSet<string>();
            actionsPlayer.Add("TurboPunch");
            actionsPlayer.Add("MiniPunch");

            // The NPC can only perform 0.3 damage at a time
            HashSet<string> actionsNPC = new HashSet<string>();
            actionsNPC.Add("Punch");
            
            enemyStrength = enemyStrength / (enemyStrength + playerStrength);
            playerStrength = playerStrength / (enemyStrength + playerStrength);
            string val = "E" + enemyHealthBar + "P" + playerHealthBar;
            if (G.containsNodeLabel(val))  {
                return G.getNodeWithUniqueLabel(val);
            }

            bool terminateRecursion = false;
            bool isAcceptingState = false;
            bool isStartingState = false;
            if (enemyHealthBar < 0.09)  {
                terminateRecursion = true;
            }
            if (playerHealthBar < 0.09)  {
                terminateRecursion = true;
                isAcceptingState = true;
            }
            if ((playerHealthBar == 1.0) && (enemyHealthBar == 1.0))  {
                isStartingState = true;
            }
            int curr = G.addVertex(val, isStartingState, isAcceptingState);
            if (!terminateRecursion)  {
                // The enemy performs an action or nothing at all.
                // - Enemy's Punch: gives the player a damage of 0.3. This is more likely than a NOOP
                int enemyPunch = setGraphRecursively(enemyHealthBar, playerHealthBar - 0.3, enemyStrength, playerStrength, eWin, eLose, ref G);
                G.addEdge(curr, enemyPunch, "enemy_punch", enemyStrength, 1);
                // - Player's TurboPunch: gives the opponent a damage of 0.5. Receiving a punch is not a big deal
                int playerTurboPunch = setGraphRecursively(enemyHealthBar - 0.5, playerHealthBar, enemyStrength, playerStrength, eWin, eLose,ref G);
                G.addEdge(curr, playerTurboPunch, "player_turbo_punch", playerStrength/2.0, -5.0);
                // - Player's MiniPunch:  gives the opponent a damage of 0.1. Receiving a punch is not a big deal
                int playerMiniPunch = setGraphRecursively(enemyHealthBar - 0.1, playerHealthBar, enemyStrength, playerStrength, eWin, eLose,ref G);
                G.addEdge(curr, playerMiniPunch, "player_mini_punch", playerStrength/2.0, -1.0);
                // - NOOP: no reward associated to this. No action reflects into a no-edge.
            } else {
                if (isAcceptingState)  {
                    G.addEdge(curr, eWin, "victory_noop", 1.0, 10);
                } else {
                    G.addEdge(curr, eLose, "loses_noop", 1.0, -10);
                }
            }
            return curr;
        }
        
        public static void main()
        {
            WeightedMultiGraph<string, string> G = new WeightedMultiGraph<string, string>();
            int eWin = G.addVertex("EnemyWins", false, true);
            int eLoses = G.addVertex("EnemyLoses");
            
            // In a very simplistic way, we can also model the MinMax solution to the realistic test through
            // a game of PONG, albeit this is not necessarily true for each minigame!

            double enemyStrength = 1.0;  // Level that might be decided at runtime
            double playerStrength = 0.0; // Level that might be decided at runtime
            double enemyFarsightedness = 1.0; // Level that might be decided at runtime

            // It is pointless to encode the graph of the possible configurations by hand for this use case example!
            // As you might see, this gets out of hand pretty quickly even for simplistic games
            int start = setGraphRecursively(1.0, 1.0, enemyStrength, playerStrength, eWin, eLoses, ref G);
            G.dot("/home/giacomo/testG.dot");
            
            PolicyIteration<string, string> policyIteration = new PolicyIteration<string, string>(G, 1.0);
            policyIteration.loop(enemyFarsightedness);
            //
            // foreach (var cp in policyIteration.V)
            //     Console.WriteLine(" Value(" + cp.Key.ToString() + ")=" + cp.Value.ToString());
            //
            // foreach (var cp in policyIteration.detPolicy)
            //     if (cp.Value != null)
            //         Console.WriteLine(" Pi("+cp.Key.ToString() + ")="+cp.Value.ToString());
            //
            // foreach (var cp in policyIteration.policy)
            // foreach (var cp2 in cp.Value)
            //     if (cp2.Key != null)
            //         Console.WriteLine(" Pi(" + cp.Key.ToString() + "|" + cp2.Key.ToString() +")=" + cp2.Value.ToString());
            //
            // Simulating a battle with the aforementioned tree.
                double scorePlayer = 1.0;
                double scoreNPC = 1.0;
                Random random  = new Random();
                long turnNo = 1;
                while (true)  {
                    Console.WriteLine("Turn #"+turnNo);
                    
                    // The first turn is always for the maximising agent: therefore, the thing I am going to do is to 
                    // simulate this action
                    var currentNode = G.resolveNode(start);
                    var currentAction = "Punch";
                    var nextId = start;
                    if (policyIteration.detPolicy[currentNode.node] == "enemy_punch")  {
                        scorePlayer -= 0.3;
                        nextId = G.resolveEdge(G.vertices[start].outgoing_edges["enemy_punch"].First()).dstId;
                    } else {
                        currentAction = "noop";
                        nextId = start;
                    }
                    
                    Console.WriteLine("NPC State Desirability: " + policyIteration.V[currentNode.node]);
                    // The best action for the enemy is always within the deterministic policy, if we don't want to 
                    // Random pick one
                    Console.WriteLine("NPC: " + currentAction);

                    if ((scorePlayer < 0.09) || (scoreNPC < 0.09))  {
                        Debug.Assert(G.resolveNode(start).getOutgoingActionNames().Contains("victory_noop") ||
                                     G.resolveNode(start).getOutgoingActionNames().Contains("loses_noop"));
                        if (scorePlayer < 0.09)
                            Console.WriteLine("Fatality: Player loses; Player="+scorePlayer+" Enemy="+scoreNPC);
                        if (scoreNPC < 0.09)
                            Console.WriteLine("Fatality: NPC loses Player="+scorePlayer+" Enemy="+scoreNPC);
                        break;
                    }
                    Console.WriteLine("Player="+scorePlayer+" Enemy="+scoreNPC);
                    start = nextId;
                    
                    // In this case, I want to get the player's best score. This is the score minimising the 
                    // Likelihood of succeeding for the opponent. This information might given to the player as
                    // a feedback as well!
                    double argMin = double.MaxValue;
                    string argName = "";
                    var label = G.resolveNode(start).node;
                    foreach (var cp2 in policyIteration.policy[label])  {
                        if (cp2.Value < argMin) {
                            argMin = cp2.Value;
                            argName = cp2.Key;
                        }
                    }
                    if (argName.Equals("player_turbo_punch"))  {
                        scoreNPC -= 0.5;
                        nextId = G.resolveEdge(G.vertices[start].outgoing_edges["player_turbo_punch"].First()).dstId;
                    } else if (argName.Equals("player_mini_punch"))  {
                        scoreNPC -= 0.1;
                        nextId = G.resolveEdge(G.vertices[start].outgoing_edges["player_mini_punch"].First()).dstId;
                    } else {
                        nextId = start;
                    }

                    Console.WriteLine("NPC State Desirability [Player Moving]: " + policyIteration.V[label]);
                    // The best action for the enemy is always within the deterministic policy, if we don't want to 
                    // Random pick one
                    Console.WriteLine("player: " + argName);
                    
                    if ((scorePlayer < 0.09) || (scoreNPC < 0.09))  {
                        Debug.Assert(G.resolveNode(start).getOutgoingActionNames().Contains("victory_noop") ||
                                     G.resolveNode(start).getOutgoingActionNames().Contains("loses_noop"));
                        if (scorePlayer < 0.09)
                            Console.WriteLine("Fatality: Player loses Player="+scorePlayer+" Enemy="+scoreNPC);
                        if (scoreNPC < 0.09)
                            Console.WriteLine("Fatality: NPC loses Player="+scorePlayer+" Enemy="+scoreNPC);
                        break;
                    }
                    start = nextId;
                    
                    Console.WriteLine("Predicted ~ "+G.resolveNode(start).node);
                    Console.WriteLine("Actual ~ Player="+scorePlayer+" Enemy="+scoreNPC);

                    turnNo++;
                }
        }
    }
}