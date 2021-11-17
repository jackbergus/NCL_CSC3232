using System;
namespace MinMaxLibrary.utils
{
    /// <summary>
    /// This class allows to mimick the recursion in an iterative way
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecursionParameters<T>
    {
        /// <summary>
        /// Input parameters for the recursion call of the algorithm
        /// </summary>
        public T input;
        /// <summary>
        /// Iterative step, corresponding to the i-th child that should be visited next, thus corresponding
        /// to the i-th recursive call of the algorithm.
        /// </summary>
        public int iterativeStep;
        /// <summary>
        /// If you want to, you might also consider a maximum depth of the recursion, as in the DeepBlue example,
        /// so to potentially stop the iteration when a maximum depth value is reached. At this point, the 
        /// scores associated to each player at this point might be used as trivial euristics upon which infer which
        /// is the player that is actually going to win the game: e.g., if my life bar is higher than the one of the
        /// opponent, I have more chances of acting upon it!
        /// </summary>
        public int depth;

        public RecursionParameters(T x)
        {
            input = x;
            iterativeStep = 0;
            depth = 0;  
        }
        public RecursionParameters(T x, int s)
        {
            input = x;
            iterativeStep = s;
            depth = 0;
        }
    }
}
