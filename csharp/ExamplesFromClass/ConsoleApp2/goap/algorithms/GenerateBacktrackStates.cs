using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp2.goap.algorithms
{
    class GenerateBacktrackStates<T>
    {
        
        ClosedWorldInference<T> cwi;
        List<WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>>> graphs;

        public GenerateBacktrackStates(ClosedWorldInference<T> cwi)
        {
            this.cwi = cwi;
            graphs = new List<WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>>>();
        }

        public List<WeightedMultiGraph<EqualityHashSet<T>, SimpleHornClause<T>>> generateGraphs(EqualityHashSet<T> Goal) {
            graphs.Clear();
            int n = Goal.Count;
            if (n >= 0) {
                graphs.Emplace(false);
                if (Goal.Count == 1) {
                    var x = Goal.First();
                    bool isAcceptingDst = cwi.init.Keys.Contains(x);
                    var dst = graphs[0].addVertex(new EqualityHashSet<T>(x), true, isAcceptingDst);
                    if (!isAcceptingDst) 
                        DFSGenerateBacktrackStates(0, Goal.First());
                } else {
                    var src = graphs[0].addVertex(Goal, true, false);
                    List<T> toExpand = new List<T>();
                    foreach (var x in Goal) {
                        bool isAcceptingDst = cwi.init.Keys.Contains(x);
                        var dst = graphs[0].addVertex(new EqualityHashSet<T>(x), true, isAcceptingDst);
                        graphs[0].addEdge(src, dst, new SimpleHornClause<T>(x));///
                        if (!isAcceptingDst) toExpand.Add(x);
                    }
                    foreach (var x in toExpand)
                        if (!DFSGenerateBacktrackStates(0, x))
                            graphs[0].errors.Add("Error on expanding for: " + x.ToString() + " over precondition = " + x.ToString());
                }
            }

            var N = 0;
            do
            {
                N = graphs.Count;
                for (int graphId = 0; graphId < N; graphId++)
                {
                    if (graphs[graphId].errors.Count == 0)
                    {
                        // Do a sanity & continue check
                        // If there are still some nodes to be visited that are not accepting ones, then continue to proceed with the visit from where
                        // the former graph left it
                        var test = new List<Int32>();
                        for (int j = 0, M = graphs[graphId].vertices.Count; j < M; j++)
                        {
                            var vertex = graphs[graphId].vertices[j];
                            if ((vertex.outgoing_edges.Count == 0) && (!vertex.accepting))
                            {
                                Debug.Assert(vertex.node.Count == 1);
                                test.Add(j);
                            }
                        }
                        foreach (var vertexId in test)
                        {
                            var x = graphs[graphId].vertices[vertexId].node.First();
                            if (!DFSGenerateBacktrackStates(graphId, x))
                                graphs[graphId].errors.Add("Error on applying rule over precondition = " + x.ToString());
                        }
                    }
                }
            } while (N != graphs.Count);

            return graphs;
        }

        bool DFSGenerateBacktrackStates(int idG, T s) {
            EqualityHashSet<T> S = new EqualityHashSet<T>(s);

            var src = graphs[idG].getNodeWithUniqueLabel(S);
            Debug.Assert(src != -1);

            ulong countFoundAlsoPartial = 0;
            var copyGraph = graphs[idG].deepCopy();
            List<SimpleHornClause<T>> validRules = new List<SimpleHornClause<T>>();
            List<Int32> graphIds = new List<int>();
            bool isFirst = true;
            foreach (var rule in cwi.rules.Keys) {
                if (rule.head.Equals(s)) {
                    validRules.Add(rule);
                    if (isFirst) {
                        isFirst = false;
                        graphIds.Add(idG);
                    } else {
                        graphIds.Add(graphs.Count);
                        graphs.Add(graphs[idG].deepCopy());
                    }
                }
            }

            for (int i = 0, N = validRules.Count; i<N; i++) {
                var rule = validRules[i];
                var graphId = graphIds[i];
                countFoundAlsoPartial++;
                List<T> progressVisiting = new List<T>();
                foreach (var x in rule.tail) {
                    EqualityHashSet<T> Sx = new EqualityHashSet<T>(x);
                    var dst = -1;
                    bool containsEdge = false;
                    if (graphs[graphId].containsNodeLabel(Sx))
                        dst = graphs[graphId].getNodeWithUniqueLabel(Sx);
                    else
                    {
                        bool isAcceptingDst = cwi.init.Keys.Contains(x);
                        dst = graphs[graphId].addVertex(Sx, false, isAcceptingDst);
                        containsEdge = graphs[graphId].containsEdge(src, dst, rule);
                        if ((!isAcceptingDst) && (!containsEdge))
                            progressVisiting.Add(x);
                    }
                    if (!containsEdge)
                        graphs[graphId].addEdge(src, dst, rule);///
                }
                foreach (var x in progressVisiting) {
                    if (!DFSGenerateBacktrackStates(graphId, x))
                        graphs[graphId].errors.Add("Error on applying rule: " + rule.ToString() + " over precondition = " + x.ToString());
                }

            }

            if (countFoundAlsoPartial == 0) {
                graphs[idG].errors.Add("Error: it was not possible to apply a rule for configuration = " + s.ToString());
                return false;
            }

            return true;
        }
    }
}
