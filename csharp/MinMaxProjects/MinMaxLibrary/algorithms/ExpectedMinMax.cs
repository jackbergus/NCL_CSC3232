using MinMaxLibrary.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinMaxLibrary.algorithms
{
    class ExpectedMinMax<ActionName, GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf
    {

        public enum IsTurnOf {
            PROBABILITY,
            MIN,
            MAX
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
            /// <summary>
            /// Update state: given the current game configuration, also containing the information of which is the player that
            /// is going to play next, and a possible action that might be performed by the player, then it returns a state 
            /// having an updated world configuration as a consequence of performing a givena ction. For example, such world configuration
            /// also contains the player's configuration, which are updated acting upon the game configuration.
            /// 
            /// If a given action cannot be performed in a given game configuration, then the function shall return an empty 
            /// optional value: the iteration will stop.
            /// 
            /// In addition to that, if the current player is the probability generator, then the double value of the optional
            /// Tuple is the probability value
            /// </summary>
            Func<CurrentGameState, ActionName, Optional<Tuple<double, CurrentGameState>>> updateState;
            /// <summary>
            /// 
            /// </summary>
            private HashSet<ActionName> opponentActions;
            private HashSet<ActionName> playerActions;
            private HashSet<ActionName> probabilisticTurnOut;
            private List<ActionName> currentPlayerList;
            private CurrentGameState currState;

            /// <summary>
            /// Class constructor
            /// </summary>
            /// <param name="x">Total generative function that updates the game progression after performing an action. If no action is returned, then the game
            /// will finish with either a winner or a tie situation</param>
            /// <param name="opponentActions">Set of all the possible actions from the opponent</param>
            /// <param name="playerActions">Set of all the possible actions from the player</param>
            public UpdateNextState(Func<CurrentGameState, ActionName, Optional<Tuple<double, CurrentGameState>>> x,
                                   HashSet<ActionName> opponentActions,
                                   HashSet<ActionName> playerActions,
                                   HashSet<ActionName> probabilisticTurnOut)
            {
                updateState = x;
                this.opponentActions = opponentActions;
                this.playerActions = playerActions;
                this.probabilisticTurnOut = probabilisticTurnOut;
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
                /// Selecting the actions from a specific player, and, in addition 
                /// to that, extracting only the actions that he might perform from the set of possible actions within this game configuration
                switch (currState.isPlayerTurn)
                {
                    case IsTurnOf.PROBABILITY:
                        currentPlayerList = probabilisticTurnOut.Where(u => updateState(currState, u).HasValue).ToList();
                        break;
                    case IsTurnOf.MIN:
                        currentPlayerList = playerActions.Where(u => updateState(currState, u).HasValue).ToList();
                        break;
                    case IsTurnOf.MAX:
                        currentPlayerList = opponentActions.Where(u => updateState(currState, u).HasValue).ToList();
                        break;
                }
                return this;
            }

            /// <summary>
            /// After setting the current state, it returns the name of the i-th action that might be potentially performed by the player
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public ActionName getActionName(int i)
            {
                return (i >= currentPlayerList.Count) ? default(ActionName) : currentPlayerList[i];
            }

            /// <summary>
            /// Given a current action id associated to the one of the actions that might be actually performed at the current
            /// game stage, I return, if it exists, the next state from which progress with the game. If not, then it means that
            /// the previous action was indeed terminal for the game (win/lose/escape)
            /// 
            /// Also, it returns the probability value if the player is the random probability action
            /// </summary>
            /// <param name="actionId"></param>
            /// <returns></returns>
            public Optional<Tuple<double, CurrentGameState>> applyFunctionNoScore(int actionId)
            {
                Debug.Assert(currState != null);
                if ((actionId < 0) || (actionId >= currentPlayerList.Count))
                    return new Optional<Tuple<double, CurrentGameState>>();
                else
                {
                    var opt = updateState(currState, currentPlayerList[actionId]);
                    if (opt.HasValue)
                        opt.Value.Item2.setActionFromParent(currentPlayerList[actionId]);
                    if (currState.isPlayerTurn != IsTurnOf.PROBABILITY)
                        Debug.Assert(opt.Value.Item1 == 1.0);
                    return opt;
                }
            }

            /// <summary>
            /// Checks whether the character playing at this current state can potentially perform an action, or not.
            /// </summary>
            /// <param name="action"></param>
            /// <returns></returns>
            public bool canCurrentPlayerPerformAction(ActionName action)
            {
                return currentPlayerList.Contains(action);
            }

            /// <summary>
            /// Same as above, but with an action id
            /// </summary>
            /// <param name="actionId"></param>
            /// <returns></returns>
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



        public class CurrentGameState : AbstractGameState<ActionName, GameConfiguration, PlayerConfiguration>
        {


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
            public IsTurnOf isPlayerTurn;
            internal double alpha;
            internal double beta;

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
                return (isPruned ? "XX" : "") + "{" + (isPlayerTurn.ToString()) +
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
            // public CurrentGameState(CurrentGameState x, IsTurnOf nextTurn, ActionName parent, double a = -double.MaxValue, double b = double.MaxValue)
            // {
            //     gameConfiguration = x.gameConfiguration;
            //     opponentLifeBar = x.opponentLifeBar;
            //     playerLifeBar = x.playerLifeBar;
            //     isPlayerTurn = nextTurn;
            //     action = new ScoreAssessment<ActionName>();
            //     parentAction = parent;
            //     alpha = a;
            //     beta = b;
            // }
            //
            // public CurrentGameState(GameConfiguration gc, PlayerConfiguration opp, PlayerConfiguration pl, IsTurnOf playerTurn)
            // {
            //     gameConfiguration = gc;
            //     opponentLifeBar = opp;
            //     playerLifeBar = pl;
            //     isPlayerTurn = playerTurn;
            //     alpha = 0.0;
            //     beta = 0.0;
            // }

            /// <summary>
            /// Returning the score as from the point of view of the enemy, that wants to maximize its overall value.
            /// If, let's say, you decide to truncate the computation of the tree to a given depth level, this will return
            /// an heuristic value for determining the likelihood of winning for the NPC given its current configuration,
            /// subtracted by the chances of winning of the player given its internal configurqation. Please observe that,
            /// in this situation, it is advised to cap the getScore values for each opponent between 0 and 1, so that 
            /// </summary>
            /// <returns></returns>
            public override double getEnemyUtilityScore()
            {
                double opp = opponentLifeBar.getScore();
                double pl = playerLifeBar.getScore();
                return opp - pl;
            }



            /// <summary>
            /// Uses the player configuration to assess whether one of the players is going to win the game.
            /// Overall, the TIE configuration is drawn from the unability of the status generator to generate 
            /// another configuration, but such requirement might change from time to time (e.g., still wins the 
            /// player/NPC that is able to maximize its score).
            /// </summary>
            /// <returns></returns>
            public override Winner whoWins()
            {
                Func<bool, bool, bool> impl = (prem, cons) => (!prem) || cons;
                var oppLost = opponentLifeBar.hasPlayerLost();
                var plLost = playerLifeBar.hasPlayerLost();
                bool oppWon;
                bool plWon;
                oppWon = opponentLifeBar.hasPlayerWon();
                plWon = playerLifeBar.hasPlayerWon();
                return basicVictoryDecision(plLost, oppWon, oppLost, plWon);
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
        /// <param name="probabilisticTurnOut">Total set of the probabilistic events that might be happening</param>
        /// <param name="updateState">Transition function from one state to the other, via an action associated to one single character at a time. If the action is probabilistic, then we also have a probability value</param>
        public ExpectedMinMax(HashSet<ActionName> opponentActions,
                      HashSet<ActionName> playerActions,
                      HashSet<ActionName> probabilisticTurnOut,
                      Func<CurrentGameState, ActionName, Optional<Tuple<double, CurrentGameState>>> updateState)
        {
            uns = new UpdateNextState(updateState, opponentActions, playerActions, probabilisticTurnOut);
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
                Optional<Tuple<double, CurrentGameState>> candidateChild = uns.applyFunctionNoScore(currentSnapshot.iterativeStep);
                /// If someone won, then we still have to stop the iteration
                bool someoneHasWon = currentSnapshot.input.data.whoWins() != Winner.TIE_OR_GAME_RUNNING;
                /// The player minimizes, the opponent maximises.
                IsTurnOf isMinimization = currentSnapshot.input.data.isPlayerTurn;


                if (currentSnapshot.iterativeStep == 0)
                { // If this is actually a function call...
                    if (someoneHasWon || (!candidateChild.HasValue))
                    {
                        // If we reached a end-of-action or win/lose state, the best action corresponds to the utility score
                        currentSnapshot.input.data.action.init(default(ActionName), currentSnapshot.input.data.getEnemyUtilityScore());
                    }
                    else
                    {                                           // Otherwise, setting up the initial values for the recursion, depending on the min/max
                        switch (isMinimization)
                        {
                            case IsTurnOf.PROBABILITY:
                                currentSnapshot.input.data.action.init(default(ActionName), 0);
                                break;
                            case IsTurnOf.MIN:
                                currentSnapshot.input.data.action.init(default(ActionName), double.MaxValue);
                                break;
                            case IsTurnOf.MAX:
                                currentSnapshot.input.data.action.init(default(ActionName), -double.MaxValue);
                                break;
                        }
                    }
                }
                else
                { // Otherwise, if I have returned from a previous recursive call, get the child value, and update the selection of the children
                  // in order to assess the score of the current node.
                    bool isAverage = isMinimization == IsTurnOf.PROBABILITY;
                    bool isMin = !isAverage;
                    double probabilityIfRequired = 1.0;
                    if (!isMin) {
                        probabilityIfRequired = candidateChild.Value.Item1;
                    } else {
                        Debug.Assert(candidateChild.Value.Item1 == 1.0); // I should have all certain events, otherwise!
                        isMin = isMinimization == IsTurnOf.MIN;
                    }

                    // Otherwise, if I have returned from a previous recursive call, get the child value, and update the selection of the children
                    // in order to assess the score of the current node.
                    currentSnapshot.input.data.action.update(retVal.data.parentAction,
                                                            probabilityIfRequired * retVal.data.action.getBestScore(),
                                                            isMin, isAverage);
                }


                if (!candidateChild.HasValue)
                {
                    // If I reached a leaf node, this implies that i would need to return the value to the caller 
                    retVal = currentSnapshot.input;
                }
                else
                {
                    // Otherwise, I prepare to create a new child
                    NTree<CurrentGameState> childTree = currentSnapshot.input.AddChild(candidateChild.Value.Item2);  // Preparing another node for the tree, containing all of the submoves
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
