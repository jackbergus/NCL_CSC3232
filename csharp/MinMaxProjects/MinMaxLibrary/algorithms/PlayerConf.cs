using System;
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
}
