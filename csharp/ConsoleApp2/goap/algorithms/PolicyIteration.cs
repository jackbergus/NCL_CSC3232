using ConsoleApp2.utils;
using System;
using System.Collections.Generic;

namespace ConsoleApp2.goap.algorithms
{
    public class PolicyIteration<NodeLabel, EdgeLabel>
    {
        public Dictionary<NodeLabel, double> V;
        public Dictionary<NodeLabel, Dictionary<EdgeLabel, double>> policy;
        public Dictionary<NodeLabel, EdgeLabel> detPolicy;
        public readonly double gamma;
        WeightedMultiGraph<NodeLabel, EdgeLabel> G;

        public PolicyIteration(WeightedMultiGraph<NodeLabel, EdgeLabel> G, double gamma, int seed = 0)
        {
            this.gamma = gamma;
            this.G = G;
            V = new Dictionary<NodeLabel, double>();    
            double lower_bound = 0;
            double upper_bound = 100;
            Random generator = new Random(seed);
            UniformRandom ur = new UniformRandom(lower_bound, upper_bound);
            foreach (var cp in G.vertices)
                if (cp.accepting)
                    V[cp.node] = 0.0;
                else
                    V[cp.node] = ur.nextReal(ref generator);
            policy = new Dictionary<NodeLabel, Dictionary<EdgeLabel, double>>();
            detPolicy = new Dictionary<NodeLabel,EdgeLabel>();
        }

        public void loop(double theta)
        {
            double Delta;
            do
            {
                Delta = 0.0;
                foreach (var s in G.vertices) {
                    double v = V[s.node];
                    double argMax = -double.MaxValue;
                    var res = G.getOutgoingActionNames(s.node);
                    if (res.Count > 0) {
                        foreach (var actionName in res) {
                            double sum = 0.0;
                            foreach (var sp in G.vertices) 
                                sum += G.getCost(gamma, V[sp.node], s.node, sp.node, actionName);
                            if (sum >= argMax)
                                argMax = sum;
                        }
                        V[s.node] = argMax;
                        Delta = Math.Max(Delta, Math.Abs(v - argMax));
                    }
                }
            } while (Delta > theta);

            foreach (var s in G.vertices)
            {
                double argMax = -double.MaxValue;
                EdgeLabel argName = default(EdgeLabel);
                foreach (var actionName in G.getOutgoingActionNames(s.node)) {
                    double sum = 0.0;
                    foreach(var sp in G.vertices) {
                        sum += G.getCost(gamma, V[sp.node], s.node, sp.node, actionName);
                    }
                    policy.GetOrInsert(s.node, () => new Dictionary<EdgeLabel, double>())[actionName] = sum;
                    if (sum >= argMax)
                    {
                        argMax = sum;
                        argName = actionName;
                    }
                }
                detPolicy[s.node] = argName;
            }
        }

    }
}
