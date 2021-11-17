using System;
namespace MinMaxLibrary.algorithms
{
    /// <summary>
    /// When we reach a final state, either one of the two player wins, or we have a tie. Still, we want to Min/Max the 
    /// score associated to that
    /// </summary>
    public enum Winner
    {
        OPPONENT_WINS,
        PLAYER_WINS,
        TIE_OR_GAME_RUNNING
    }
}
