using ConsoleApp2.goap.structures;
using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.goap.algorithms
{
    public class GenerateWeightedPossibleStates<T> {
        ClosedWorldInference<T> cwi;
        Func<double, double> weightTransformation;
        Func<List<double>, double> weightCombine;

        public GenerateWeightedPossibleStates(ClosedWorldInference<T> cwi, 
                                              Func<double, double> weightTransformation, 
                                              Func<List<double>, double> weightCombine) { 
            this.cwi = cwi;
            this.weightTransformation = weightTransformation;
            this.weightCombine = weightCombine;
        }

        public GenerateWeightedPossibleStates(ClosedWorldInference<T> cwi) {
            this.cwi = cwi;
            this.weightTransformation = ExpWeight;
            this.weightCombine = SumCombine;
        }


        /// <summary>
        /// This function preserves the weight value\
        /// </summary>
        public static Func<double, double> IDWeight = i => i;

        public static Func<double, double> IDNormalize = i => i / (i + 1.0);
        public static Func<double, double> ExpWeight = i => Math.Exp(i);
        public static Func<List<double>, double> SumCombine = i => i.Sum();
        public static Func<List<double>, double> OrCombine = i => 1.0 - i.Aggregate(1.0, (v, a) => a *= (1 - v));
        

        Int32 DFSGenerateEdgeWeightedPossibleStates(ref WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>> G,
                                     EqualityHashSet<T> S,
                                     EqualityHashSet<T> Goal,
                                     bool isStarting = false)
        {
            if (G.containsNodeLabel(S))
            {
                return G.getNodeWithUniqueLabel(S);
            }
            else
            {
                bool isAccepting = Goal.IsSubsetOf(S);
                var src = G.addUniqueVertexByLabel(S, isStarting, isAccepting, -1.0);

                List<double> weightCollect = new List<double>();
                List<Int32> edgeIds = new List<Int32>();

                foreach (var rule in cwi.rules)
                {
                    if ((rule.Key.tail.IsSubsetOf(S)) && (!S.Contains(rule.Key.head)))
                    {
                        double w = weightTransformation(rule.Value);
                        weightCollect.Add(w);

                        EqualityHashSet<T> tmp = new EqualityHashSet<T>(S);
                        tmp.Add(rule.Key.head);
                        Int32 dst = DFSGenerateEdgeWeightedPossibleStates(ref G, tmp, Goal);
                        Int32 edgeId = G.addEdge(src, dst, rule.Key, w);
                    }
                }
                double totalWeight = weightCombine(weightCollect);
                weightCollect.Clear();
                foreach (var edgeId in edgeIds)
                {
                    G.edges[edgeId].probability = G.edges[edgeId].probability / totalWeight;
                }
                edgeIds.Clear();
                return src;
            }
        }

        public WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>> GenerateFullyWeightedPossibleStates(EqualityHashSet<T> Goal)
        {
            var G = new WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>>();
            int start = DFSGenerateEdgeWeightedPossibleStates(ref G, cwi.init.Keys.ToEqualityHashSet(), Goal, true);
            Debug.Assert(G.inverseEdges[start].Count == 0);
            Debug.Assert(G.starters.Contains(start));
            G.vertices[start].weight = G.vertices[start].node.Aggregate(1.0, (acc, elem) => acc * cwi.init[elem]);
            foreach (int vertex in G.TopologicalSort()) {
                if (vertex != start) {
                    G.vertices[start].weight =  1.0 - G.inverseEdges[vertex].Aggregate(1.0, (acc, edgeId) =>
                    {
                        var w = G.vertices[G.edges[edgeId].srcId].weight;
                        Debug.Assert(w != -1.0);
                        return acc * (1.0 - (w * G.edges[edgeId].probability));
                    });
                }
            }
            return G;
        }
    }
}
