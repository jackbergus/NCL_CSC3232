using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConsoleApp2.goap
{

    public class WeightedMultiGraph<NodeLabel, EdgeLabel> {

        /// <summary>
        /// Specifies the whole vertex information
        /// </summary>
        /// <typeparam name="NodeLabel">Type associated to node labels/contents</typeparam>
        /// <typeparam name="EdgeLabel">Type associated to edge labels/conents</typeparam>
        public class Vertex<NodeLabel, EdgeLabel>
        {
            public NodeLabel node;
            /// <summary>
            /// For deterministic graphs, if there exists a key within the map, the outgoing edge shall have only one single label.
            /// </summary>
            public Dictionary<EdgeLabel, HashSet<Int32>> outgoing_edges;
            public readonly bool starting;
            public readonly bool accepting;
            public double weight;

            /// <summary>
            /// Creates a vertex
            /// </summary>
            /// <param name="label">Label/content associated to the node</param>
            /// <param name="isStarting">Whether it is a starting node from which start the visit within the graph</param>
            /// <param name="isAccepting">Whether it is a node that, when reached, the executed is accepted by the environment</param>
            /// <param name="weight">Weight associated to the vertex (it could be also a desirability of reaching the node)</param>
            public Vertex(NodeLabel label, bool isStarting = false, bool isAccepting = false, double weight = 1.0)
            {
                this.weight = weight;
                node = label;
                starting = isStarting;
                accepting = isAccepting;    
                outgoing_edges = new Dictionary<EdgeLabel, HashSet<Int32>>();
            }

            public Vertex(Vertex<NodeLabel, EdgeLabel> x) {
                starting = x.starting;
                accepting = x.accepting;
                weight = x.weight;
                outgoing_edges = new Dictionary<EdgeLabel, HashSet<Int32>>(x.outgoing_edges.Count);
                node = x.node;
                foreach (var kv in x.outgoing_edges)
                    outgoing_edges[kv.Key] = new HashSet<int>(kv.Value.ToArray());
            }

            public Vertex<NodeLabel, EdgeLabel> deepCopy()
            {
                return new Vertex<NodeLabel, EdgeLabel>(this);
            }

            public HashSet<EdgeLabel> getOutgoingActionNames()
            {
                return outgoing_edges.Keys.ToHashSet();
            }
        }

        /// <summary>
        /// Representing an edge with its label
        /// </summary>
        /// <typeparam name="EdgeLabel">Type associated to the edge label/content</typeparam>
        public class Edge<EdgeLabel>
        {
            public int srcId { get; }
            public int dstId { get; }
            public EdgeLabel label;
            public double probability;
            public double reward;

            public Edge(int srcId, int dstId, EdgeLabel label, double probability = 1.0, double reward = 0.0)
            {
                this.srcId = srcId;
                this.dstId = dstId;
                this.label = label;
                this.probability = probability;
                this.reward = reward;
            }

            public Edge(Edge<EdgeLabel> x)
            {
                srcId = x.srcId;
                dstId = x.dstId;
                label = x.label;
                probability = x.probability;
                reward = x.reward;
            }

            public Edge<EdgeLabel> deepCopy()
            {
                return new Edge<EdgeLabel>(this);
            }
        }

        /// <summary>
        /// In order to have efficient maps from node-id to node content, we store vertices in vectors 
        /// </summary>
        public List<Vertex<NodeLabel, EdgeLabel>> vertices;
        /// <summary>
        /// In order to have efficient maps from edge-id to eedge content, we store edges in vectors 
        /// </summary>
        public List<Edge<EdgeLabel>> edges;
        /// <summary>
        /// Associating each node label/content to all the vertices having that label
        /// </summary>
        public Dictionary<NodeLabel, EqualityHashSet<Int32>> vertexLabels;
        /// <summary>
        /// Represents the ingoing edges for each node in the graph. By doing so, we are also able to traverse the graph backwards
        /// </summary>
        public List<List<Int32>> inverseEdges;
        /// <summary>
        /// Represents the nodes from which we can start the navigation
        /// </summary>
        public EqualityHashSet<Int32> starters { get;  }
        /// <summary>
        /// Represents the nodes which, when reached, accept the navigation as a correct one
        /// </summary>
        public EqualityHashSet<Int32> accepters { get;  }
        /// <summary>
        /// The sets of possible errors that were encountered while creating the graph
        /// </summary>
        public List<string> errors;
        /// <summary>
        /// Whether the edges shall be deterministic or not
        /// </summary>
        public readonly bool isDeterministic;

        /// <summary>
        /// Creating an empty graph
        /// </summary>
        public WeightedMultiGraph(bool isDeterministic = true)
        {
            vertices = new List<Vertex<NodeLabel, EdgeLabel>>();
            edges = new List<Edge<EdgeLabel>> ();
            vertexLabels = new Dictionary<NodeLabel, EqualityHashSet<Int32>>();
            inverseEdges = new List<List<Int32>> ();
            starters = new EqualityHashSet<Int32>();
            accepters = new EqualityHashSet<Int32>();
            errors = new List<string>();
            this.isDeterministic = isDeterministic;
        }

        /// <summary>
        ///  Providing a deep copy of the data structure
        /// </summary>
        /// <param name="x"></param>
        public WeightedMultiGraph(WeightedMultiGraph<NodeLabel, EdgeLabel> x)
        {
            vertices = new List<Vertex<NodeLabel, EdgeLabel>>(x.vertices.Count);
            foreach (var y in x.vertices)
                vertices.Add(y.deepCopy());

            edges = new List<Edge<EdgeLabel>>(x.edges.Count);
            foreach (var y in x.edges)
                edges.Add(y.deepCopy());

            vertexLabels = new Dictionary<NodeLabel, EqualityHashSet<Int32>>(x.vertexLabels.Count);
            foreach (var y in x.vertexLabels)
                vertexLabels.Add(y.Key, new EqualityHashSet<Int32>(y.Value));

            inverseEdges = new List<List<Int32>>(x.inverseEdges);
            foreach (var y in x.inverseEdges)
            {
                List<Int32> ls = new List<int>();
                foreach (var z in y)
                    ls.Add(z);
                inverseEdges.Add(ls);  
            }

            starters = new EqualityHashSet<Int32>(x.starters);
            accepters = new EqualityHashSet<Int32>(x.accepters);

            errors = new List<string>(x.errors.Count);
            foreach (var y in x.errors)
                errors.Add(y);

            isDeterministic = x.isDeterministic;
        }

        /// <summary>
        /// Serializes the graph into a dot file that can be used in GraphViz to be visualized
        /// </summary>
        /// <param name="filename"></param>
        public void dot(string filename)
        {
            var file = new StreamWriter(filename);
            file.WriteLine("digraph finite_state_machine {");
            file.WriteLine("    rankdir=LR;");
            file.WriteLine("    size=\"8,5\"");
            int node_id = 0;
            for (int i = 0, V =  vertices.Count; i < V; i++) {
                string shape = "circle";
                string root = "";
                if (accepters.Contains(i))
                    shape = "doublecircle";
                if (starters.Contains(i))
                {
                    file.WriteLine("fake" + i.ToString() + " [style=invisible]");
                    root = "root=true, ";
                }
                file.WriteLine( "node [" + root + "label=\"" + vertices[i].node.ToString()+ "\", shape=" + shape + ", fontsize=10] q"+ i.ToString());
            }
            foreach (int i in starters) {
                file.WriteLine("fake" + i.ToString() + " -> q" + i.ToString() + " [style=bold]");
            }
            for (int  i = 0, E = edges.Count; i < E; i++) {
                file.WriteLine("q" + edges[i].srcId.ToString() + " -> q" + edges[i].dstId.ToString() + " [style=bold, label=\""+ edges[i].label.ToString()+"\"]");
            }
            file.WriteLine("}");
            foreach (var error in errors)
                file.WriteLine("#" + error);
            file.Close();
        }

        /// <summary>
        /// Performs the deepCopy of the object, without explicitly invoking the constructor
        /// </summary>
        /// <returns></returns>
        public WeightedMultiGraph<NodeLabel, EdgeLabel> deepCopy()
        {
            return new WeightedMultiGraph<NodeLabel, EdgeLabel>(this);
        }

        /// <summary>
        /// Adds a vertex to the graph
        /// </summary>
        /// <param name="label"></param>
        /// <param name="isStarting"></param>
        /// <param name="isAccepting"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public Int32 addVertex(NodeLabel label, bool isStarting = false, bool isAccepting = false, double weight = 1.0)
        {
            Int32 id = vertices.Count;
            vertices = (List<Vertex<NodeLabel,EdgeLabel>>)CppStructUtils.Emplace(vertices, label, isStarting, isAccepting, weight);
            if (!vertexLabels.ContainsKey(label))
                vertexLabels[label] = new EqualityHashSet<Int32>();
            vertexLabels[label].Add(id);
            inverseEdges.Add(new List<Int32>());
            if (isStarting) starters.Add(id);
            if (isAccepting) accepters.Add(id);
            return id;
        }

        /// <summary>
        /// Returns whether it already exists a node in the graph with the same label
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public bool containsNodeLabel(NodeLabel label)
        {
            return vertexLabels.ContainsKey(label);
        }

        /// <summary>
        /// Under the assumption that there is a bijection between nodes and their labels, returns the node id associated to the label
        /// </summary>
        /// <param name="label"></param>
        /// <returns>If there exists the vertex label in the graph, then returns the node-id; if there isn't, it returns -1. If there
        /// are multiple nodes with the same label, that breaks the assumption, and therefore the program is terminated</returns>
        public Int32 getNodeWithUniqueLabel(NodeLabel label) {
            if (vertexLabels.ContainsKey(label)) {
                Debug.Assert(vertexLabels[label].Count == 1);
                return vertexLabels[label].First(); 
            }
            else return -1;
        }

        /// <summary>
        /// If there already exists a vertex with a given label, while ignoring whether the node is starting/accepting, then it
        /// returns the id of the previously-created node. Otherwise, a novel node is generated and its id returned.
        /// </summary>
        /// <param name="label">Node label</param>
        /// <param name="isStarting">Whether it is a starting node</param>
        /// <param name="isAccepting">Whether it is an accepting node</param>
        /// <param name="weight">Weight associated to the node</param>
        /// <returns></returns>
        public Int32 addUniqueVertexByLabel(NodeLabel label, bool isStarting = false, bool isAccepting = false, double weight = 1.0)
        {
            if (containsNodeLabel(label)) {
                var s = vertexLabels[label];
                Debug.Assert(s.Count == 1);
                return s.First();
            } else {
                return addVertex(label, isStarting, isAccepting, weight);
            }
        }

        public Int32 addEdge(int srcId, int dstId, EdgeLabel label, double weight = 1.0, double reward = 0.0)
        {
            
            Debug.Assert(srcId < vertices.Count, "src id is not valid");
            Debug.Assert(dstId < vertices.Count, "dst id is not valid");
            if (isDeterministic) Debug.Assert(!vertices[srcId].outgoing_edges.ContainsKey(label), "Label " + label.ToString() + " already found, and going to (previously inserted) ???, vs. proposed (current attempt) "); //"+ edges[vertices.ElementAt(srcId).outgoing_edges[label].First()].dstId.ToString()+"



            Int32 id = edges.Count;
            edges = (List<Edge<EdgeLabel>>)CppStructUtils.Emplace(edges, srcId, dstId, label, weight, reward);

            vertices.ElementAt(srcId).outgoing_edges.GetOrInsert(label, () => new HashSet<int>()).Add(id);
            inverseEdges[dstId].Add(id);
            return id;
        }

        internal bool containsEdge(int src, int dst, EdgeLabel rule)
        {
            if (src >= vertices.Count) return false;
            if (dst >= vertices.Count) return false;
            if (!vertices[src].outgoing_edges.ContainsKey(rule)) return false;
            foreach (var edgeId in vertices[src].outgoing_edges[rule])
                if (edges[edgeId].dstId == dst) return true;
            return false;
        }


        public Edge<EdgeLabel> resolveEdge(Int32 edgeId)
        {
            Debug.Assert(edgeId < edges.Count);
            return edges.ElementAt(edgeId);
        }

        public Vertex<NodeLabel, EdgeLabel> resolveNode(Int32 nodeId)
        {
            Debug.Assert(nodeId < vertices.Count);
            return vertices.ElementAt(nodeId);
        }

        // A recursive function used by topologicalSort
        void TopologicalSortUtil(int v, bool[] visited,
                                 Stack<int> stack)
        {

            // Mark the current node as visited.
            visited[v] = true;

            // Recur for all the vertices

            // adjacent to this vertex
            foreach (var edgesWithSameLabel in vertices[v].outgoing_edges.Values) {
                foreach (var edge in edgesWithSameLabel) {
                    var dst = edges[edge].dstId;
                    if (!visited[dst])
                        TopologicalSortUtil(dst, visited, stack);
                }
            }

            // Push current vertex to
            // stack which stores result
            stack.Push(v);
        }

        public HashSet<EdgeLabel> getOutgoingActionNames(NodeLabel l)
        {
            if (containsNodeLabel(l))
                return vertices[getNodeWithUniqueLabel(l)].getOutgoingActionNames();
            else
                return new HashSet<EdgeLabel>();
        }

        public double getCost(double gamma, double dstV, NodeLabel src, NodeLabel dst, EdgeLabel action)
        {
            if (containsNodeLabel(src) && containsNodeLabel(dst)) {
                var srcX = getNodeWithUniqueLabel(src);
                var dstX = getNodeWithUniqueLabel(dst);
                if (!vertices[srcX].outgoing_edges.ContainsKey(action)) return 0.0;
                else {
                    foreach (var edgeId in vertices[srcX].outgoing_edges[action])
                        if (edges[edgeId].dstId == dstX)
                            return edges[edgeId].probability * (edges[edgeId].reward + gamma * dstV);
                    return 0.0;
                }
            }
            else return 0.0;
        }

        /// <summary>
        /// Performs the topological sort of the graph.
        /// 
        /// I.E., a node precedes the other if there is a path 
        /// </summary>
        /// <returns></returns>
        public Stack<int> TopologicalSort()
        {
            Stack<int> stack = new Stack<int>();

            // Mark all the vertices as not visited
            var visited = new bool[vertices.Count];

            // Call the recursive helper function
            // to store Topological Sort starting
            // from all vertices one by one
            foreach (int i in starters) {
                if (visited[i] == false)
                    TopologicalSortUtil(i, visited, stack);
            }

            return stack;
        }

    }
}
