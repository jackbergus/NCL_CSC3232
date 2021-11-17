using System;
namespace MinMaxLibrary.algorithms
{
    public class ScoreAssessment<ActionName>
    {

        private Tuple<ActionName, double> bestGlobalAction;

        public ScoreAssessment()
        {
            bestGlobalAction = null;
        }

        /// <summary>
        /// Setting the initial value, just to start the minimization/maximization task
        /// </summary>
        /// <param name="action">(potentially a) Bogus action name</param>
        /// <param name="score">(potentially a) Bogus score to start the minimization/maximization </param>
        internal void init(ActionName action, double score)
        {
            bestGlobalAction = new Tuple<ActionName, double>(action, score);
        }

        /// <summary>
        /// Updates an action with a given value only if it either the minimum/maximum available for the current round
        /// </summary>
        /// <param name="action">Action that is currently assessed</param>
        /// <param name="score">Score associated to the action being performed</param>
        /// <param name="isMin">Whether the best score should be associated to a minimum value or to a maximum one</param>
        internal void update(ActionName action, double score, bool isMin)
        {
            if ((isMin && (score < bestGlobalAction.Item2)) || ((!isMin) && (score > bestGlobalAction.Item2)))
            {
                bestGlobalAction = new Tuple<ActionName, double>(action, score);
            }
        }

        /// <summary>
        /// Returning the score associated to the best action so far
        /// </summary>
        /// <returns></returns>
        public double getBestScore()
        {
            return bestGlobalAction.Item2;
        }

        /// <summary>
        /// Returning the best action visited so far
        /// </summary>
        /// <returns></returns>
        public ActionName getBestAction()
        {
            return bestGlobalAction.Item1;
        }
    }
}
