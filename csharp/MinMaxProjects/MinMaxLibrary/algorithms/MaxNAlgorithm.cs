using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MinMaxLibrary.utils;

namespace MinMaxLibrary.algorithms
{
    public class MaxNAlgorithm<ActionName, GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf {
        public class Player
        {
            public HashSet<ActionName> actions;
            public PlayerConfiguration lifeBar;
            public bool isAntagnoistAndMax;

            public Player(HashSet<ActionName> actions, PlayerConfiguration lifeBar, bool isAntagnoistAndMax) {
                this.actions = actions;
                this.lifeBar = lifeBar;
                this.isAntagnoistAndMax = isAntagnoistAndMax;
            }

            public Player clone()
            {
                return new Player(actions, lifeBar, isAntagnoistAndMax);
            }
        }


        public class CurrentGameState
        {
            public GameConfiguration gameConfiguration;
            public List<Player> players;
            public int playerIdTurn;
            public ActionName   bestAction;
            public List<double> bestUtilityVector;
            public ActionName parentAction;

            private String printUtilityVector()
            {
                return "<"+String.Join(",", bestUtilityVector.Select((x) => x.ToString()).ToArray())+">";
                
            }

            public override string ToString()
            {
                return "{player=" + playerIdTurn +
                       "; totalScore = " + printUtilityVector() +
                       "; parentAction = " + parentAction +"}";
            }

            public CurrentGameState(CurrentGameState x)
            {
                gameConfiguration = x.gameConfiguration;
                players = new List<Player>();
                foreach (var y in x.players)
                    players.Add(y.clone());
                playerIdTurn = x.playerIdTurn;
                bestAction = x.bestAction;
                bestUtilityVector = x.bestUtilityVector;    
                parentAction = x.parentAction; 
            }

            public CurrentGameState()
            {
                gameConfiguration = default(GameConfiguration);
                players = new List<Player>();
                playerIdTurn = -1;
                bestAction = default(ActionName);
                bestUtilityVector = new List<double>();
                parentAction = default(ActionName);
            }

            public CurrentGameState(CurrentGameState x, ActionName parent, int playerIdTurn/*, double a = -double.MaxValue, double b = double.MaxValue*/)
            {
                gameConfiguration = x.gameConfiguration;
                players = new List<Player>();
                foreach (var y in x.players)
                    players.Add(y.clone());
                this.playerIdTurn = playerIdTurn;
                parentAction = parent;
                /*alpha = a;
                beta = b;*/
            }

            public CurrentGameState(GameConfiguration gc, List<Player> p, int playerIdTurn)
            {
                gameConfiguration = gc;
                players = new List<Player>();
                foreach (Player p2 in p) players.Add(p2.clone());
                this.playerIdTurn = playerIdTurn;
            }

            /// <summary>
            /// Returning the score as from the point of view of the enemy, that wants to maximize its overall value.
            /// If, let's say, you decide to truncate the computation of the tree to a given depth level, this will return
            /// an heuristic value for determining the likelihood of winning for the NPC given its current configuration,
            /// subtracted by the chances of winning of the player given its internal configurqation. Please observe that,
            /// in this situation, it is advised to cap the getScore values for each opponent between 0 and 1, so that 
            /// </summary>
            /// <returns></returns>
            public double getEnemyUtilityScore()
            {
                double total = 0.0;
                foreach (var p in players) 
                    total += (p.isAntagnoistAndMax ? 1.0 : -1.0) * p.lifeBar.getScore();
                return total;
            }

            internal bool haveAllEnemiesLost() {
                return players.FindAll(x => x.isAntagnoistAndMax).TrueForAll(x => x.lifeBar.hasPlayerLost());
            }

            internal bool haveAllHelpersLost() {
                return players.FindAll(x => !x.isAntagnoistAndMax).TrueForAll(x => x.lifeBar.hasPlayerLost());
            }

            internal bool hasOneEnemyWon() {
                return players.FindAll(x => x.isAntagnoistAndMax).Exists(x => x.lifeBar.hasPlayerWon());
            }

            internal bool hasOneHelperWon() {
                return players.FindAll(x => !x.isAntagnoistAndMax).Exists(x => x.lifeBar.hasPlayerWon());
            }

            /// <summary>
            /// Optional, for backward compatibility with the MPD. Returns a local reward for transitioning from a given configuration to another
            /// </summary>
            /// <param name="nextStep">Status immediately following the current one</param>
            /// <returns>reward score associated to the transitioning</returns>
            public double getLocalRewardForTransition(CurrentGameState nextStep)
            {
                //Debug.Assert(playerIdTurn != nextStep.playerIdTurn);
                return nextStep.getEnemyUtilityScore() - getEnemyUtilityScore();
            }

            /// <summary>
            /// Uses the player configuration to assess whether one of the players is going to win the game.
            /// Overall, the TIE configuration is drawn from the unability of the status generator to generate 
            /// another configuration, but such requirement might change from time to time (e.g., still wins the 
            /// player/NPC that is able to maximize its score).
            /// </summary>
            /// <returns></returns>
            public Winner whoWins()
            {
                Func<bool, bool, bool> impl = (prem, cons) => (!prem) || cons;
                var oppLost = haveAllEnemiesLost();
                var plLost = haveAllHelpersLost();
                bool oppWon;
                bool plWon;
                oppWon = hasOneEnemyWon();
                plWon = hasOneHelperWon();
                // If one loses, the other one wins automatically
                if (plLost)
                {
                    oppWon = true;
                    oppLost = false;
                    plWon = false;
                }
                if (oppLost)
                {
                    plWon = true;
                    plLost = false;
                    oppWon = false;
                }
                if ((plLost && oppLost) || (plWon && oppWon))
                    return Winner.TIE_OR_GAME_RUNNING;

                // If I don't have a tie, then the game is still running (noone has won yet) or there is only one winner
                Debug.Assert(((!plLost) && (!oppLost)) || (plWon && (oppLost)) || (oppWon && (plLost)));

                if (oppWon)
                    return Winner.OPPONENT_WINS;
                else if (plWon)
                    return Winner.PLAYER_WINS;
                else
                    return Winner.TIE_OR_GAME_RUNNING;
            }

            internal void setActionFromParent(ActionName actionName)
            {
                parentAction = actionName;
            }

            /// <summary>
            /// Differently from the MinMax algorithm, where we have one global value to minimise/maximise, in this situation
            /// each player will only minimise or maximise his onw value. Then, we are required to provide an utility vector 
            /// </summary>
            /// <returns></returns>
            public List<double> getUtilityVector()
            {
                List<double> vs = new List<double>(players.Count);
                for (int i = 0, N = players.Count; i<N; i++) 
                {
                    vs.Add(players[i].lifeBar.getScore());
                }
                return vs;
            }
        }

        /// <summary>
        /// This class performs the state generation for a given configuration. Also this class associates the number of tentative
        /// iteration calls to generate a next child to the number of all the possible actions that might be performed
        /// by a player/character in a given configuration. By doing so, we can use a generic recursion algorithm that is independent
        /// from the characterization of the action name, and only depends to the possible total number of actions that a character
        /// can perform in a given state
        /// </summary>
        class UpdateNextState
        {
            private Func<CurrentGameState, ActionName, Optional<CurrentGameState>> updateState;
            private List<HashSet<ActionName>> characterActions;
            private List<ActionName> currentPlayerList;
            private CurrentGameState currState;

            /// <summary>
            /// Class constructor
            /// </summary>
            /// <param name="x">Total generative function that updates the game progression after performing an action. If no action is returned, then the game
            /// will finish with either a winner or a tie situation</param>
            public UpdateNextState(Func<CurrentGameState, ActionName, Optional<CurrentGameState>> x,
                                   List<Player> players)
            {
                updateState = x;
                characterActions = new List<HashSet<ActionName>>();
                foreach (var y in players)
                    characterActions.Add(y.actions);
                currentPlayerList = null;
                currState = null;
            }

            /// <summary>
            /// Current state from which we want to generate states
            /// </summary>
            /// <returns></returns>
            public CurrentGameState getCurrentState()
            {
                return currState;
            }

            /// <summary>
            /// This is the method actually setting the whole logic of the software
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public UpdateNextState setCurrentState(CurrentGameState x)
            {
                /// Setting up the current state, so to retrieve it when needed
                currState = x;
                if (currentPlayerList == null)
                    currentPlayerList = new List<ActionName>();
                if ((currState.playerIdTurn < 0) || (currState.playerIdTurn >= x.players.Count))
                {
                    currentPlayerList.Clear();
                } else 
                /// Selecting the actions from a specific player, and, in addition 
                /// to that, extracting only the actions that he might perform from the set of possible actions within this game configuration
                currentPlayerList = characterActions[currState.playerIdTurn].Where(u => updateState(currState, u).HasValue).ToList();
                return this;
            }

            public ActionName getActionName(int i)
            {
                return (i >= currentPlayerList.Count) ? default(ActionName) : currentPlayerList[i];
            }

            public Optional<Tuple<CurrentGameState, double>> applyFunction(int actionId)
            {
                Debug.Assert(currState != null);
                if ((actionId < 0) || (actionId >= currentPlayerList.Count))
                    return new Optional<Tuple<CurrentGameState, double>>();
                else
                {
                    var opt = updateState(currState, currentPlayerList[actionId]);
                    if (opt.HasValue) {
                        opt.Value.setActionFromParent(currentPlayerList[actionId]);
                        return new Optional<Tuple<CurrentGameState, double>>(new Tuple<CurrentGameState, double>(opt.Value, currState.getLocalRewardForTransition(opt.Value)));
                    }
                    else
                        return new Optional<Tuple<CurrentGameState, double>>();
                }
            }

            public Optional<CurrentGameState> applyFunctionNoScore(int actionId)
            {
                Debug.Assert(currState != null);
                if ((actionId < 0) || (actionId >= currentPlayerList.Count))
                    return new Optional<CurrentGameState>();
                else
                {
                    var opt = updateState(currState, currentPlayerList[actionId]);
                    if (opt.HasValue)
                        opt.Value.setActionFromParent(currentPlayerList[actionId]);
                    return opt;
                }
            }

            public bool canCurrentPlayerPerformAction(ActionName action)
            {
                return currentPlayerList.Contains(action);
            }

            public bool canCurrentPlayerPerformAction(int actionId)
            {
                return currentPlayerList.Count >= (actionId);
            }

            public CurrentGameState getCurrentGameState()
            {
                return currState;
            }

            public int getActionId(ActionName value)
            {
                return currentPlayerList.FindIndex(a => a.Equals(value));
            }
        }


        UpdateNextState uns;

       public MaxNAlgorithm(List<Player> players,
                      Func<CurrentGameState, ActionName, Optional<CurrentGameState>> updateState)
        {
            uns = new UpdateNextState(updateState, players);
        }


        /// <summary>
        /// This action fits the whole model. This solution, which visits the set of all the possible states, might be used similarly to the
        /// MDP model for multiplayers, i.e., by loading the whole model in advance, and then providing all the possible suggestions after loading the
        /// whole model. For the runtime decision process, where you might feed directly a given configuration
        /// </summary>
        /// <returns></returns>
        public NTree<CurrentGameState> fitModel(CurrentGameState cgs)
        {
            NTree<CurrentGameState> retVal = null;
            Stack<RecursionParameters<NTree<CurrentGameState>>> stack = new Stack<RecursionParameters<NTree<CurrentGameState>>>();

            // Mimicking the first call to the recursive function
            stack.Push(new RecursionParameters<NTree<CurrentGameState>>(new NTree<CurrentGameState>(cgs)));

            while (stack.Count > 0)
            {
                /// Either starting a new recursive call, or recovering from a former child call
                RecursionParameters<NTree<CurrentGameState>> currentSnapshot = stack.Pop();
                /// Setting up the action selectors to the actions that might be performed from the given play configuration
                uns.setCurrentState(currentSnapshot.input.data);
                /// Returning a child that might be called in the next iterative step. If none is returned, then we stop the iteration
                Optional<CurrentGameState> candidateChild = uns.applyFunctionNoScore(currentSnapshot.iterativeStep);
                /// If someone won, then we still have to stop the iteration
                bool someoneHasWon = (currentSnapshot.input.data.whoWins() != Winner.TIE_OR_GAME_RUNNING) ||
                                     (currentSnapshot.input.data.playerIdTurn == -1);


                bool reachedLeafNode = (someoneHasWon || (!candidateChild.HasValue));
                if (currentSnapshot.iterativeStep == 0)
                { // If this is actually a function call...
                    currentSnapshot.input.data.bestAction = default(ActionName);
                    if (reachedLeafNode)
                    {
                        // If we reached a end-of-action or win/lose state, the best action corresponds to the utility score

                        currentSnapshot.input.data.bestUtilityVector = currentSnapshot.input.data.getUtilityVector();
                    }
                    else
                    {                                           // Otherwise, setting up the initial values for the recursion, depending on the min/max
                        currentSnapshot.input.data.bestUtilityVector = null;
                    }
                }
                else
                { // Otherwise, if I have returned from a previous recursive call, get the child value, and update the selection of the children
                  // in order to assess the score of the current node.
                    if ((currentSnapshot.input.data.bestUtilityVector == null) ||
                        (retVal.data.bestUtilityVector[currentSnapshot.input.data.playerIdTurn]
                        > currentSnapshot.input.data.bestUtilityVector[currentSnapshot.input.data.playerIdTurn]))
                    {

                        currentSnapshot.input.data.bestAction = retVal.data.bestAction;
                        currentSnapshot.input.data.bestUtilityVector = retVal.data.bestUtilityVector;
                    }
                }


                if (reachedLeafNode)
                {
                    // If I reached a leaf node, this implies that i would need to return the value to the caller 
                    retVal = currentSnapshot.input;
                }
                else
                {
                    // Otherwise, I prepare to create a new child
                    ///Console.WriteLine("[" + currentSnapshot.input.data.parentAction + "]->[" +candidateChild.Value.parentAction+"]");
                    NTree<CurrentGameState> childTree = currentSnapshot.input.AddChild(candidateChild.Value);  // Preparing another node for the tree, containing all of the submoves
                    childTree.data.setActionFromParent(uns.getActionName(currentSnapshot.iterativeStep));      // Setting up the parent action (it should be already set by applyFunctionNoScore,
                                                                                                               // but we are enforcing such a condition.
                    currentSnapshot.iterativeStep++;                                                           // preparing for the next recursive call, where we would need to call the next child
                    stack.Push(currentSnapshot);                                                               // pushing the updated state to the stack
                    var child = new RecursionParameters<NTree<CurrentGameState>>(childTree);
                    child.depth = currentSnapshot.depth + 1;                                                   // Just for debugging purposes: looking at the stack grow!
                    stack.Push(child);                                                                         // Calling the child for the first time
                }
            }

            return retVal;
        }
    }
}
