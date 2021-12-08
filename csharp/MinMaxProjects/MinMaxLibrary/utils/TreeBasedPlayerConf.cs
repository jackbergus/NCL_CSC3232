using MinMaxLibrary.algorithms;

namespace MinMaxProjects.tests
{

    /// <summary>
    /// Example class of keeping specifically track of the character information. In this case, we are basically
    /// ignoring the score, but that score might as well be associated to the life bar of either the character
    /// or of the opponent.
    /// </summary>
    public class TreeBasedPlayerConf : PlayerConf
    {
        double x;
        bool isLostWhenScoreIsZero, hasWonWhenScoreIsOne;

        /// <summary>
        /// If you have a life bar, this score might be the initialized value associated to the score.
        /// </summary>
        /// <param name="var"></param>
        public TreeBasedPlayerConf(double var, bool isLostWhenScoreIsZero = false, bool hasWonWhenScoreIsOne = false)
        {
            x = var;
            this.isLostWhenScoreIsZero = isLostWhenScoreIsZero;
            this.hasWonWhenScoreIsOne = hasWonWhenScoreIsOne;
        }

        /// <summary>
        /// Initialize the initial score. 
        /// </summary>
        public TreeBasedPlayerConf()
        {
            x = 0.0;
        }
        public override double getScore()
        {
            return x;
        }

        /// <summary>
        /// In this trivial example, none of the characters will actually win or lose, rather than the game
        /// will stop after two turns. Then, the player will still try to maximise (or minimise) the total
        /// score of the game. 
        /// 
        /// For 2 players battling, you might consider to normalize the life bar to 1, and then make the enemy 
        /// win if the score is greater than zero (it would imply that the player lost more life than the enemy)
        /// and let him lose if the score is less than zero.
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool hasPlayerWon()
        {
            return hasWonWhenScoreIsOne ? (System.Math.Abs(x - 1.0) < 0.001) : false;
        }

        /// <summary>
        /// When two players are battling, then one player wins if the opponent loses. Therefore, the hasPlayerWon
        /// might potentially always return false. In this case, the winner will be automatically inferred by the
        /// whoWins method.
        /// </summary>
        /// <returns></returns>
        public override bool hasPlayerLost()
        {
            return isLostWhenScoreIsZero ? (System.Math.Abs(x) < 0.001) : false;
        }

        public override PlayerConf clone()
        {
            return new TreeBasedPlayerConf(x, isLostWhenScoreIsZero, hasWonWhenScoreIsOne);
        }
    }
}
