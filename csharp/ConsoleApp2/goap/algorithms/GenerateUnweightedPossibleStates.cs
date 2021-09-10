using ConsoleApp2.utils;
using System;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp2.goap.algorithms
{
    public class GenerateUnweightedPossibleStates<T> {
        ClosedWorldInference<T> cwi;
        private readonly bool debug;

        public GenerateUnweightedPossibleStates(ClosedWorldInference<T> cwi, bool debug = false) {
            this.cwi = cwi;
            this.debug = debug;
        }

        Int32 DFSGenerateUnweightedPossibleStates(ref WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>> G,
                                             EqualityHashSet<T> S,
                                             EqualityHashSet<T> Goal,
                                             bool isInitial = false)
        {
            if (G.containsNodeLabel(S))
            {
                if (debug) Console.WriteLine("Line Match: " + S.ToString() + " vs. " + G.vertices[G.getNodeWithUniqueLabel(S)].node.ToString());
                Debug.Assert(G.vertices[G.getNodeWithUniqueLabel(S)].node.Equals(S));
                return G.getNodeWithUniqueLabel(S);
            }
            else
            {
                bool isAccepting = Goal.IsSubsetOf(S);
                var src = G.addUniqueVertexByLabel(S, isInitial, isAccepting);
                foreach (var rule in cwi.rules)
                {
                    if ((rule.Key.tail.IsSubsetOf(S)) && (!S.Contains(rule.Key.head)))
                    {
                        EqualityHashSet<T> tmp = new EqualityHashSet<T>(S);
                        tmp.Add(rule.Key.head);
                        Int32 dst = DFSGenerateUnweightedPossibleStates(ref G, tmp, Goal);
                        G.addEdge(src, dst, rule.Key);
                    }
                }
                return src;
            }
        }

        /// <summary>
        /// Generates an unweighted graph, where each state represents a possible game configuration of resources/goals unlocked, and
        /// each transition represents a rule that was applied to change the state. In the process, we ignore the rules that do not update
        /// the state information
        /// </summary>
        /// <param name="Goal"> Goal to be reached by applying the rules</param>
        /// <returns>An unweighted graph</returns>
        public WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>> generateUnweightedPossibleStates(EqualityHashSet<T> Goal)
        {
            WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>> G = new WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>>();
            DFSGenerateUnweightedPossibleStates(ref G, new EqualityHashSet<T>(cwi.init.Keys.ToArray()), Goal, true);
            return G;
        }

    }
}
