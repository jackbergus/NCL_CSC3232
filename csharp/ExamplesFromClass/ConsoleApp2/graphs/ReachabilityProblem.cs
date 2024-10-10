using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp2.utils;

namespace ConsoleApp2.graphs
{


    public interface Graph<T>
    {
        Dictionary<int, T> vertices();
        Dictionary<int, double> getNeighboursOf(int src);
    }

    public class ConcreteGraph : Graph<Pair<int, int>>
    {

        List<Pair<int, int>> nodes;
        Dictionary<int, Dictionary<int, double>> adjacencyList;

        public Pair<int, int> GetNode(int idx)
        {
            return nodes[idx];
        }

        public ConcreteGraph()
        {
            nodes = new List<Pair<int,int>>();
            adjacencyList = new Dictionary<int, Dictionary<int, double>>();
        }

        public int addNode(int x, int y)
        {
            var id = nodes.Count;
            nodes.Add(new Pair<int, int>(x,y));
            return id;
        }

        public void addEdge(int src, double weight, int dst)
        {
            if (!adjacencyList.ContainsKey(src))
            {
                adjacencyList[src] = new Dictionary<int, double>();
            }
            adjacencyList[src][dst] = weight;
        }

        public Dictionary<int, double> getNeighboursOf(int src)
        {
            if (!adjacencyList.ContainsKey(src))
                return new Dictionary<int, double>();
            return adjacencyList[src];
        }

        public Dictionary<int, Pair<int, int>> vertices()
        {
            Dictionary<int, Pair<int, int>> d = new Dictionary<int, Pair<int, int>>();
            for (int i = 0; i<nodes.Count; i++)
            {
                d[i] = nodes[i];
            }
            return d;
        }
    }





    public class ReachabilityProblem<T>
    {
        Dictionary<int, double> distances;
        Dictionary<int, int> parent;

        public ReachabilityProblem()
        {
            distances = new Dictionary<int, double>(); // gScore for A*
            parent = new Dictionary<int, int>(); // cameFrom for A*
        }

        void init(int src)
        {
            distances.Clear();
            parent.Clear();
            distances[src] = 0;
        }

        public static double GetDistances(Dictionary<int,double> distances, int val)
        {
            if (distances.ContainsKey(val))
                return distances[val];
            else
                return Double.MaxValue;
        }

        public double GetDistance(int val)
        {
            if (distances.ContainsKey(val))
                return distances[val];
            else
                return Double.MaxValue;
        }

        public int GetParent(int val)
        {
            if (parent.ContainsKey(val))
                return parent[val];
            else
                return -1;
        }

        Tuple<List<int>, double> crawlFrom(int src, int target)
        {
            List<int> S = new List<int>();
            double totalCost = Double.MaxValue;
            distances.TryGetValue(target, out totalCost);
            var uu = target;
            if (((parent.ContainsKey(uu)) && (parent[uu] != -1)) || (uu == src))
            {
                while (uu != -1)
                {
                    S.Insert(0, uu);
                    uu = GetParent(uu);
                }
            }
            return new Tuple<List<int>, double>(S, totalCost);
        }

        public Tuple<List<int>, double> Astar(Graph<T> graph, int src, int target, Func<int,double> h)
        {
            init(src);
            HashSet<int> openSet = new HashSet<int>();          // set of nodes to be visited
            openSet.Add(src);   
            HashSet<int> closedSet = new HashSet<int>();        // set of visited nodes
            var fscore = new Dictionary<int, double>();
            fscore[src] = h(src);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(item => GetDistances(fscore, item)).ToList()[0]; 
                if (current == target)
                {
                    break;
                }
                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var outEdge in graph.getNeighboursOf(current))
                {
                    if (!closedSet.Contains(outEdge.Key)) {
                        var alt = GetDistance(current) + outEdge.Value;
                        if (alt < GetDistance(outEdge.Key) || (!openSet.Contains(outEdge.Key))) 
                        {
                            distances[outEdge.Key] = alt;
                            parent[outEdge.Key] = current;
                            fscore[outEdge.Key] = alt + h(outEdge.Key);
                            if (!openSet.Contains(outEdge.Key))
                            {
                                openSet.Add(outEdge.Key);
                            }
                        }
                    }
                }
            }


            return crawlFrom(src, target);
        }


        public Tuple<List<int>, double> Dijkstra(Graph<T> graph, int src, int target)
        {

            init(src);
            List<int> Q = new List<int>(); // Keeping the non-visited nodes
            foreach (var x in graph.vertices())
            {
                Q.Add(x.Key);
            }

            while (Q.Count > 0)
            {
                Q = Q.OrderBy(item => GetDistance(item)).ToList();
                var u = Q[0];
                if (u == target)
                    break;
                Q.RemoveAt(0);


                foreach (var outEdge in graph.getNeighboursOf(u))
                {
                    if (Q.Contains(outEdge.Key)) // edge Target: ensuring that the node has not been visited
                    {
                        var alt = GetDistance(u) + outEdge.Value; // edge Weight
                        if (alt < GetDistance(outEdge.Key))
                        {
                            distances[outEdge.Key] = alt;
                            parent[outEdge.Key] = u;
                        }
                    }
                }
            }

            return crawlFrom(src, target);
        }


        public static void ReachabilityTest()
        {
            var g = new ConcreteGraph();
            g.addNode(0, 0);
            g.addNode(1, 1);
            g.addNode(1, -1);
            g.addNode(2, 0);
            g.addNode(3, -1);
            g.addNode(3, 1);
            g.addNode(4, 0);
            g.addEdge(0, 2.0, 1);
            g.addEdge(0, 6.0, 2);
            g.addEdge(1, 5.0, 3);
            g.addEdge(2, 8.0, 3);
            g.addEdge(3, 15.0, 5);
            g.addEdge(3, 10.0, 4);
            g.addEdge(5, 6.0, 4);
            g.addEdge(5, 6.0, 6);
            g.addEdge(4, 2.0, 6);
            var algorithms = new ReachabilityProblem<Pair<int, int>>();
            var (dijPath, dijCost) = algorithms.Dijkstra(g, 0, 6);
            Console.WriteLine("[{0}] with cost {1}", string.Join(", ", dijPath), dijCost);
            var (aPath, aCost) = algorithms.Astar(g, 0, 6, idx => Math.Sqrt(Math.Pow(g.GetNode(idx).key - 16,2)+Math.Pow(g.GetNode(idx).value,2)));
            Console.WriteLine("[{0}] with cost {1}", string.Join(", ", aPath), aCost);
            Console.WriteLine("OK");
        }
    }
}
