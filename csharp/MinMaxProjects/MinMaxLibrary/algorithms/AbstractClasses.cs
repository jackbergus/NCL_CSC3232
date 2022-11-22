using System;
using System.Diagnostics;

namespace MinMaxLibrary.algorithms
{
    public abstract class PlayerConf {

        /// <summary>
        /// If a game is of type "gotta catch 'em all" style, then the player won (for sure) if he was able to collect
        /// all of the items from the game, or at least the requested amount (normalized between 0 and 1). This approach is
        /// dual and there opposite of the other one assumed in the next method...
        /// </summary>
        /// <returns></returns>
        public virtual bool hasPlayerWon()
        {
            return Math.Abs(getScore() - 1.0) < 0.0001;
        }

        /// <summary>
        /// If a game is a battle and the score represents the total live, the player loses when the live is near to zero
        /// </summary>
        /// <returns></returns>
        public virtual bool hasPlayerLost()
        {
            return Math.Abs(getScore()) < 0.0001;
        }

        /// <summary>
        /// Functtion returning the percieved score associated to a player. E.g., this might be the lifebar associated to
        /// a player during a battle.
        /// </summary>
        /// <returns></returns>
        public abstract double getScore();

        public abstract PlayerConf clone();
    }

    public abstract class AbstractUpdateNextState<ActionName, GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf
    {
        public AbstractGameState<ActionName, GameConfiguration, PlayerConfiguration> currState;
        
        /// <summary>
        /// Current state from which we want to generate states
        /// </summary>
        /// <returns></returns>
        public AbstractGameState<ActionName, GameConfiguration, PlayerConfiguration>  getCurrentState()
        {
            return currState;
        }
        public abstract AbstractUpdateNextState<ActionName, GameConfiguration, PlayerConfiguration> setCurrentState(AbstractGameState<ActionName, GameConfiguration, PlayerConfiguration> x);
    }
    
    public abstract class AbstractGameState<ActionName, GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf
    {
        /// <summary>
        /// Configuration of the whole game
        /// </summary>
        public GameConfiguration gameConfiguration;
        
        public ActionName parentAction;
        
        /// <summary>
        /// Accumulator for assessing the best action met so far
        /// </summary>
        public ScoreAssessment<ActionName> action;

        public abstract double getEnemyUtilityScore();

        /// <summary>
        /// Optional, for backward compatibility with the MPD. Returns a local reward for transitioning from a given configuration to another
        /// </summary>
        /// <param name="nextStep">Status immediately following the current one</param>
        /// <returns>reward score associated to the transitioning</returns>
        public double getLocalRewardForTransition(AbstractGameState<ActionName, GameConfiguration, PlayerConfiguration> nextStep)
        {
            return nextStep.getEnemyUtilityScore() - getEnemyUtilityScore();
        }

        public abstract Winner whoWins();
        
        public static Winner basicVictoryDecision(bool plLost, bool oppWon, bool oppLost, bool plWon)
        {
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

    public abstract class AbstractMinMax<ActionName, GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf
    {
    

    }
}
