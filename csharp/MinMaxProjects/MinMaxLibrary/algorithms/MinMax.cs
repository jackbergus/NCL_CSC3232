using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MinMaxLibrary.utils;

namespace MinMaxLibrary.algorithms {
    public class MinMax<ActionName, GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf {



        /// <summary>
        /// This class performs the state generation for a given configuration. Also this class associates the number of tentative
        /// iteration calls to generate a next child to the number of all the possible actions that might be performed
        /// by a player/character in a given configuration. By doing so, we can use a generic recursion algorithm that is independent
        /// from the characterization of the action name, and only depends to the possible total number of actions that a character
        /// can perform in a given state
        /// </summary>
        class UpdateNextState {
            /// <summary>
            /// Update state: given the current game configuration, also containing the information of which is the player that
            /// is going to play next, and a possible action that might be performed by the player, then it returns a state 
            /// having an updated world configuration as a consequence of performing a givena ction. For example, such world configuration
            /// also contains the player's configuration, which are updated acting upon the game configuration.
            /// 
            /// If a given action cannot be performed in a given game configuration, then the function shall return an empty 
            /// optional value: the iteration will stop 
            /// </summary>
            private Func<CurrentGameState, ActionName, Optional<CurrentGameState>> updateState;
            /// <summary>
            /// 
            /// </summary>
            private HashSet<ActionName> opponentActions;
            private HashSet<ActionName> playerActions;
            private List<ActionName> currentPlayerList;
            private CurrentGameState currState;

            /// <summary>
            /// Class constructor
            /// </summary>
            /// <param name="x">Total generative function that updates the game progression after performing an action. If no action is returned, then the game
            /// will finish with either a winner or a tie situation</param>
            /// <param name="opponentActions">Set of all the possible actions from the opponent</param>
            /// <param name="playerActions">Set of all the possible actions from the player</param>
            public UpdateNextState(Func<CurrentGameState, ActionName, Optional<CurrentGameState>> x,
                                   HashSet<ActionName> opponentActions,
                                   HashSet<ActionName> playerActions) {
                updateState = x;
                this.opponentActions = opponentActions;
                this.playerActions = playerActions;
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
            public UpdateNextState setCurrentState(CurrentGameState x) {
                /// Setting up the current state, so to retrieve it when needed
                currState = x;
                /// Selecting the actions from a specific player, and, in addition 
                /// to that, extracting only the actions that he might perform from the set of possible actions within this game configuration
                currentPlayerList = currState.isPlayerTurn ? 
                               playerActions.Where(u => updateState(currState, u).HasValue).ToList() : 
                               opponentActions.Where(u => updateState(currState, u).HasValue).ToList();
                return this;
            }

            /// <summary>
            /// After setting the current state, it returns the name of the i-th action that might be potentially performed by the player
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public ActionName getActionName(int i) {
                return (i >= currentPlayerList.Count) ? default(ActionName) : currentPlayerList[i];
            }

            /// <summary>
            /// Given a current action id associated to the one of the actions that might be actually performed at the current
            /// game stage, I return, if it exists, the next state from which progress with the game. If not, then it means that
            /// the previous action was indeed terminal for the game (win/lose/escape)
            /// </summary>
            /// <param name="actionId"></param>
            /// <returns></returns>
            public Optional<CurrentGameState> applyFunctionNoScore(int actionId) {
                Debug.Assert(currState != null);
                if ((actionId < 0) || (actionId >= currentPlayerList.Count))
                    return new Optional<CurrentGameState>();
                else {
                    var opt = updateState(currState, currentPlayerList[actionId]);
                    if (opt.HasValue)
                        opt.Value.setActionFromParent(currentPlayerList[actionId]);
                    return opt;
                }
            }

            /// <summary>
            /// Same method as the previous but, at this stage, we also return the score associated to the action
            /// </summary>
            /// <param name="actionId"></param>
            /// <returns></returns>
            public Optional<Tuple<CurrentGameState, double>> applyFunction(int actionId)
            {
                Debug.Assert(currState != null);
                if ((actionId < 0) || (actionId >= currentPlayerList.Count))
                    return new Optional<Tuple<CurrentGameState, double>>();
                else
                {
                    var opt = updateState(currState, currentPlayerList[actionId]);
                    if (opt.HasValue)
                    {
                        Debug.Assert(opt.Value.isPlayerTurn != currState.isPlayerTurn);
                        opt.Value.setActionFromParent(currentPlayerList[actionId]);
                        return new Optional<Tuple<CurrentGameState, double>>(new Tuple<CurrentGameState, double>(opt.Value, currState.getLocalRewardForTransition(opt.Value)));
                    }
                    else
                        return new Optional<Tuple<CurrentGameState, double>>();
                }
            }

            /// <summary>
            /// Checks whether the character playing at this current state can potentially perform an action, or not.
            /// </summary>
            /// <param name="action"></param>
            /// <returns></returns>
            public bool canCurrentPlayerPerformAction(ActionName action) {
                return currentPlayerList.Contains(action);
            }

            /// <summary>
            /// Same as above, but with an action id
            /// </summary>
            /// <param name="actionId"></param>
            /// <returns></returns>
            public bool canCurrentPlayerPerformAction(int actionId) {
                return currentPlayerList.Count >= (actionId);
            }

            public CurrentGameState getCurrentGameState() {
                return currState;
            }

            public int getActionId(ActionName value) {
                return currentPlayerList.FindIndex(a => a.Equals(value));
            }
        }



        public class CurrentGameState {
            /// <summary>
            /// Configuration of the whole game
            /// </summary>
            public GameConfiguration gameConfiguration;

            /// <summary>
            /// Opponent's configuration
            /// </summary>
            public PlayerConfiguration opponentLifeBar;

            /// <summary>
            /// Player's configuration
            /// </summary>
            public PlayerConfiguration playerLifeBar;

            /// <summary>
            /// Set to true if it is the player's turn, and to falso if it is the opponent's
            /// </summary>
            public bool isPlayerTurn;

            /// <summary>
            /// Accumulator for assessing 
            /// </summary>
            public ScoreAssessment<ActionName> action;
            internal double alpha;
            internal double beta;
            public ActionName parentAction;

            public bool isPruned = false; 

            public static String printAlphaBeta(double x)
            {
                if (x == -double.MaxValue)
                    return "-∞";
                else if (x == double.MaxValue)
                    return "+∞";
                else
                    return x.ToString();
            }

            public override string ToString()
            {
                return (isPruned ? "XX" : "") + "{" + (isPlayerTurn ? "MIN" : "MAX") +
                       "; totalScore = " + opponentLifeBar.getScore() +
                       "; parentAction = " + parentAction +
                       "; bestScore = " + action.getBestScore() +
                       "; α,β = " + printAlphaBeta(alpha) + "," + printAlphaBeta(beta) + "}";
            }

            public CurrentGameState()
            {
                action = new ScoreAssessment<ActionName>();
                alpha = -double.MaxValue;
                beta = double.MaxValue;
            }
            public CurrentGameState(CurrentGameState x, ActionName parent, double a = -double.MaxValue, double b = double.MaxValue)
            {
                gameConfiguration = x.gameConfiguration;
                opponentLifeBar = x.opponentLifeBar;
                playerLifeBar = x.playerLifeBar;
                isPlayerTurn = !x.isPlayerTurn;
                action = new ScoreAssessment<ActionName>();
                parentAction = parent;
                alpha = a;
                beta = b;
            }

            public CurrentGameState(GameConfiguration gc, PlayerConfiguration opp, PlayerConfiguration pl, bool playerTurn)
            {
                gameConfiguration = gc;
                opponentLifeBar = opp;
                playerLifeBar = pl;
                isPlayerTurn = playerTurn;
                alpha = 0.0;
                beta = 0.0;
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
                double opp = opponentLifeBar.getScore();
                double pl = playerLifeBar.getScore();
                return opp - pl;
            }

            /// <summary>
            /// Optional, for backward compatibility with the MPD. Returns a local reward for transitioning from a given configuration to another
            /// </summary>
            /// <param name="nextStep">Status immediately following the current one</param>
            /// <returns>reward score associated to the transitioning</returns>
            public double getLocalRewardForTransition(CurrentGameState nextStep)
            {
                Debug.Assert(isPlayerTurn != nextStep.isPlayerTurn);
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
                Func<bool, bool, bool> impl = (prem,cons) => (!prem) || cons;
                var oppLost = opponentLifeBar.hasPlayerLost();
                var plLost = playerLifeBar.hasPlayerLost();
                bool oppWon;
                bool plWon;
                oppWon = opponentLifeBar.hasPlayerWon();
                plWon = playerLifeBar.hasPlayerWon();
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
        }

        UpdateNextState uns;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MinMaxLibrary.MinMax`3"/> class.
        /// </summary>
        /// <param name="opponentActions">Total set of the Opponent's actions.</param>
        /// <param name="playerActions">Total set of the Player's actions.</param>
        /// <param name="updateState">Transition function from one state to the other, via an action associated to one single character at a time</param>
        public MinMax(HashSet<ActionName> opponentActions, 
                      HashSet<ActionName> playerActions, 
                      Func<CurrentGameState, ActionName, Optional<CurrentGameState>> updateState) {
            uns = new UpdateNextState(updateState, opponentActions, playerActions);
        }

        /// <summary>
        /// 
        /// This function provides a partial plan to the player invoking the model with the current world configuration.
        /// Please observe that some of the nodes might be pruned, and so if those nodes were not generated, it simply
        /// means that the opponent acted upon an action that is not one of the best possible worlds for the former player...
        /// </summary>
        /// <param name="cgs"></param>
        /// <returns></returns>
        public NTree<CurrentGameState> fitWithAlphaBetaPruning(CurrentGameState cgs) {
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
                bool someoneHasWon = currentSnapshot.input.data.whoWins() != Winner.TIE_OR_GAME_RUNNING;
                /// The player minimizes, the opponent maximises.
                bool isMinimization = currentSnapshot.input.data.isPlayerTurn;
                /// As a default behaviour, I never prune
                bool alphaBetaPruning = false;


                bool reachedLeafNode = (someoneHasWon || (!candidateChild.HasValue));

                if (currentSnapshot.iterativeStep == 0)
                { // If this is actually a function call...
                    if (reachedLeafNode)
                    {
                        // If we reached a end-of-action or win/lose state, the best action corresponds to the utility score
                        double myScore = currentSnapshot.input.data.getEnemyUtilityScore();
                        currentSnapshot.input.data.alpha = myScore;
                        currentSnapshot.input.data.beta = myScore;
                        currentSnapshot.input.data.action.init(default(ActionName), myScore);
                    } else {                                           
                        // Otherwise, setting up the initial values for the recursion, depending on the min/max
                        currentSnapshot.input.data.action.init(default(ActionName), isMinimization ? double.MaxValue : -double.MaxValue);
                    }
                }
                else
                { 

                    // Otherwise, if I have returned from a previous recursive call, get the child value, and update the selection of the children
                  // in order to assess the score of the current node.
                    currentSnapshot.input.data.action.update(retVal.data.parentAction,
                                                            retVal.data.action.getBestScore(),
                                                            isMinimization);

                    alphaBetaPruning = ((isMinimization && (currentSnapshot.input.data.action.getBestScore() <= currentSnapshot.input.data.alpha))
                                        ||
                                        ((!isMinimization) && (currentSnapshot.input.data.action.getBestScore() >= currentSnapshot.input.data.beta))
                                       );
                    if (alphaBetaPruning)
                        currentSnapshot.input.data.isPruned = true;
                }


                if (alphaBetaPruning || (reachedLeafNode))
                {
                    // If I reached a leaf node, this implies that i would need to return the value to the caller 
                    retVal = currentSnapshot.input;
                }
                else
                {
                    if (isMinimization)
                        currentSnapshot.input.data.beta = Math.Min(currentSnapshot.input.data.beta, currentSnapshot.input.data.action.getBestScore());
                    else
                        currentSnapshot.input.data.alpha = Math.Max(currentSnapshot.input.data.alpha, currentSnapshot.input.data.action.getBestScore());
                    // Ensuring the correctness of the application of the alpha-beta pruning assessment, if any
                    candidateChild.Value.alpha = currentSnapshot.input.data.alpha;
                    candidateChild.Value.beta = currentSnapshot.input.data.beta;

                    // Otherwise, I prepare to create a new child
                    ///Console.WriteLine("[" + currentSnapshot.input.data.parentAction + "]->[" + candidateChild.Value.parentAction + "]");
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

        /// <summary>
        /// This action fits the whole model. This solution, which visits the set of all the possible states, might be used similarly to the
        /// MDP model for multiplayers, i.e., by loading the whole model in advance, and then providing all the possible suggestions after loading the
        /// whole model. For the runtime decision process, where you might feed directly a given configuration
        /// </summary>
        /// <returns></returns>
        public NTree<CurrentGameState> fitModel(CurrentGameState cgs) {
            NTree<CurrentGameState> retVal = null;
            Stack<RecursionParameters<NTree<CurrentGameState>>> stack = new Stack<RecursionParameters<NTree<CurrentGameState>>>();

            // Mimicking the first call to the recursive function
            stack.Push(new RecursionParameters<NTree<CurrentGameState>>(new NTree<CurrentGameState>(cgs)));

            while (stack.Count > 0) {
                /// Either starting a new recursive call, or recovering from a former child call
                RecursionParameters<NTree<CurrentGameState>> currentSnapshot = stack.Pop();
                /// Setting up the action selectors to the actions that might be performed from the given play configuration
                uns.setCurrentState(currentSnapshot.input.data);
                /// Returning a child that might be called in the next iterative step. If none is returned, then we stop the iteration
                Optional<CurrentGameState> candidateChild = uns.applyFunctionNoScore(currentSnapshot.iterativeStep);
                /// If someone won, then we still have to stop the iteration
                bool someoneHasWon = currentSnapshot.input.data.whoWins() != Winner.TIE_OR_GAME_RUNNING;
                /// The player minimizes, the opponent maximises.
                bool isMinimization = currentSnapshot.input.data.isPlayerTurn;

                bool reachedLeafNode = (someoneHasWon || (!candidateChild.HasValue));
                if (currentSnapshot.iterativeStep == 0) { // If this is actually a function call...
                    if (reachedLeafNode) { 
                        // If we reached a end-of-action or win/lose state, the best action corresponds to the utility score
                        currentSnapshot.input.data.action.init(default(ActionName), currentSnapshot.input.data.getEnemyUtilityScore());
                    } else {                                           // Otherwise, setting up the initial values for the recursion, depending on the min/max
                        currentSnapshot.input.data.action.init(default(ActionName), isMinimization ? double.MaxValue : -double.MaxValue);
                    }
                } else { // Otherwise, if I have returned from a previous recursive call, get the child value, and update the selection of the children
                         // in order to assess the score of the current node.
                    currentSnapshot.input.data.action.update(retVal.data.parentAction, 
                                                            retVal.data.action.getBestScore(),
                                                            isMinimization);
                }


                if (reachedLeafNode) {
                    // If I reached a leaf node, this implies that i would need to return the value to the caller 
                    retVal = currentSnapshot.input;
                } else {
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
