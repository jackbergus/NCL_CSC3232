using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp2.goap.algorithms;
using ConsoleApp2.utils;

namespace ConsoleApp2.goap
{



    public class Edge
    {
        public Room src, dst;
        public bool hasDoor;

        public Edge(Room src, Room dst) {
            this.src = src;
            this.dst = dst;
            this.hasDoor = false;

            double dx = dst.x_c - src.x_c;
            double dy = dst.y_c - src.y_c;
        }


        double Signed2DTriArea(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            return (Ax - Cx) * (By - Cy) - (Ay - Cy) * (Bx - Cx);
        }
        bool TestSegmentSegment(double Ax, double Ay, double Bx, double By, 
                                  double Cx, double Cy, double Dx, double Dy)
        {
            double a1 = Signed2DTriArea(Ax, Ay, Bx, By, Dx, Dy);
            double a2 = Signed2DTriArea(Ax, Ay, Bx, By, Cx, Cy);
            if (a1 * a2 < 0.0)
            {
                double a3 = Signed2DTriArea(Cx, Cy, Dx, Dy, Ax, Ay);
                double a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0) {
                    return true;
                }
            }
            return false;
        }


        public bool intersectsWith(ref Edge lineB)
        {
            return TestSegmentSegment(src.x_c, src.y_c, dst.x_c, dst.y_c,
                lineB.src.x_c, lineB.src.y_c, lineB.dst.x_c, lineB.dst.y_c);
        }
    }

    public class Room
    {
        int width;
        int height;
        public int left;//left
        public int bottom;//bottom
        public int right;//right
        public int top;//tom
        public double x_c;
        public double y_c;
        public HashSet<utils.Pair<int, int>> resources_idest_keys;

        public Room(int width, int height, int x, int y) // width>0, height>0
        {
            this.x_c = x + ((double)width) / 2.0;
            this.y_c = y + ((double)height) / 2.0;
            this.width = width;
            this.height = height;
            this.left = x;
            this.bottom = y;
            this.right = x + width;
            this.top = y + height;
            resources_idest_keys = new HashSet<utils.Pair<int, int>>();
        }

        public bool segment_intersection(double maxA, double maxB, double minA, double minB)
        {
            double rA = (2+ maxA - minA) / 2.0;
            double rB = (2 + maxB - minB) / 2.0;
            double midA = minA + rA;
            double midB = minB + rB;
            double middist = Math.Abs(midA - midB);
            return (middist <= Math.Abs(rA + rB));
        }

        public bool intersectsWith(ref Room r2)
        {
            return segment_intersection(right, r2.right, left, r2.left) &&
                segment_intersection(top, r2.top, bottom, r2.bottom);
        }
    }

    public class ProceduralDungeon
    {
        public int starting_room, ending_room;
        public int n_rooms;
        public int r_width;
        public int r_height;
        public int l_width;
        public int l_height;
        Random rng;
        List<Room> rooms;
        List<Edge> corridors;
        ConsoleApp2.utils.UniformRandom ur_r, ur_l, ur_R, rup;
        HashSet<ConsoleApp2.utils.Pair<int, int>> fullCells, edges;
        List<HashSet<int>> adj;
        Dictionary<ConsoleApp2.utils.Pair<int, int>, int> cellToRoom;
        Dictionary<ConsoleApp2.utils.Pair<int, int>, Edge> edgeMap;

        public ProceduralDungeon(int rooms_max,
                            int room_width,
                            int room_height,
                            int level_width,
                            int level_height) {
            n_rooms = rooms_max;
            r_width = room_width;
            r_height  = room_height;
            l_width = level_width;
            l_height = level_height;
            rng = new Random();
            ur_r = new ConsoleApp2.utils.UniformRandom(r_width, r_height);
            ur_l = new ConsoleApp2.utils.UniformRandom(l_width, l_height);
            ur_R = new ConsoleApp2.utils.UniformRandom(0, rooms_max-1);
            rup = new ConsoleApp2.utils.UniformRandom(0, 1.0);
            rooms = new List<Room>(rooms_max);
            corridors = new List<Edge>(rooms_max * rooms_max - rooms_max);
            fullCells = new HashSet<utils.Pair<int, int>>();
            edges = new HashSet<utils.Pair<int, int>>();
            adj = new List<HashSet<int>>();
            starting_room = -1;
            ending_room = -1;
            cellToRoom = new Dictionary<utils.Pair<int, int>, int>();
            edgeMap = new Dictionary<utils.Pair<int, int>, Edge>();
        }

        Room generateRoom()
        {
            var width = Math.Max(2, ur_r.nextInt(ref rng));
            var height = Math.Max(2, ur_r.nextInt(ref rng));
            var x = ur_l.nextInt(ref rng, 0, l_width - width - 1);
            var y = ur_l.nextInt(ref rng, 0, l_height - height - 1);
            return new Room(width, height, x, y);
        }

        bool intersects(ref List<Room> rooms, ref Room r) {
            foreach (var room_other in rooms) {
                if (room_other.intersectsWith(ref r)) {
                    return true;
                }
            }
            return false;
        }

        bool intersects(ref List<Edge> edges, ref Edge e) {
            foreach (var edge in edges) {
                if (edge.intersectsWith(ref e)) {
                    return true;
                }
            }
            return false;
        }


        /**
         * Associating a resource (key) to a door
         **/
        public void addKeyForDoorInRoom(int roomId, int door_src, int door_dst) {
            if (roomId < rooms.Count) {
                rooms[roomId].resources_idest_keys.Add(new utils.Pair<int, int>(door_src, door_dst));
            }
        }

        /**
         * Adding a door to an edge
         **/
        public void setLockedDoorOnEdge(int door_src, int door_dst) {
            var edge = new utils.Pair<int, int>(door_src, door_dst);
            if (edgeMap.ContainsKey(edge)) {
                edgeMap[edge].hasDoor = true;
            }
        }

        /**
         * Checking whether an existing edge has a door associated to it
         **/
        public bool hasEdgeLockedDoor(int door_src, int door_dst) {
            var edge = new utils.Pair<int, int>(door_src, door_dst);
            if (edgeMap.ContainsKey(edge))
            {
                return edgeMap[edge].hasDoor;
            }
            else
                return false;
        }


        void addRoom(ref Room r, int i) {
            rooms.Add(r);
            adj.Add(new HashSet<int>());
            for (int x = r.left; x<=r.right; x++) {
                for (int y = r.bottom; y<=r.top; y++) {
                    var cp = new utils.Pair<int, int>(x, y);
                    cellToRoom.Add(cp, i);
                    fullCells.Add(cp);
                }
            }
        }

        void addCorridor(int src, int dst, ref Edge e) {
            corridors.Add(e);
            edgeMap.Add(new utils.Pair<int, int>(src, dst), e);
            // Straightforward Rasterisation Algorithm
            int x2 = (int)Math.Round(e.src.x_c);
            int y2 = (int)Math.Round(e.src.y_c);
            int x1 = (int)Math.Round(e.dst.x_c);
            int y1 = (int)Math.Round(e.dst.y_c);
            int dx = x2 - x1;
            int dy = y2 - y1;
            double lineLength = Math.Sqrt(dx * dx + dy * dy);
            double repr = 1.0 / (lineLength - 1.0);
            double t = 0.0;
            for (int i = 0; i<Math.Ceiling(lineLength); i++)
            {
                int x = (int)((double)x1 * (1 - t) + ((double)x2 * t));
                int y = (int)((double)y1 * (1 - t) + ((double)y2 * t));
                fullCells.Add(new utils.Pair<int, int>(x, y));
                t += repr;
            }
        }

        public void printOnTerminal() {
            utils.Pair<int, int> p;
            for (int x = 0; x<=this.l_width; x++)
            {
                for (int y = this.l_height; y>0; y--)
                {
                    p = new utils.Pair<int, int>(x, y);
                    if (!fullCells.Contains(p))
                        Console.Write("X");
                    else
                        if (cellToRoom.ContainsKey(p))
                        Console.Write(cellToRoom[p]);
                    else
                        Console.Write(" ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Start: " + starting_room);
            Console.WriteLine("End: " + ending_room);
        }

        public Pair<int, int[]> maxDeg()
        {
            int maxVal = -1;
            int maxCand = 0;
            int[] vecMax = null;
            for (int v= 0; v<n_rooms; v++)
            {
                var vec = Dijkstra.dijkstra(adj, v);
                int maxValVec = -1;
                for (int i = 0; i<vec.Length; i++)
                {
                    var val = vec[i];
                    if ((val < int.MaxValue) && (val > 0) && (val > maxValVec))
                    {
                        maxValVec = val;
                    }
                }
                if (maxValVec > maxVal)
                {
                    vecMax = vec;
                    maxVal = maxValVec;
                    maxCand = v;
                }
            }
            return new Pair<int, int[]>(maxCand, vecMax);
        }

        public static String doorName(utils.Pair<int, int> p) {
            return "d(" + p.key + "," + p.value + ")";
        }

        public static String keyForDoor(utils.Pair<int, int> p) {
            return "k(" + p.key + "," + p.value + ")";
        }

        public Pair<bool,List<List<string>>> isFeasable() {
            var ap = new AllPaths(ref adj);
            var candidate_end_door = new HashSet<string>();
            var resources = new EqualityHashSet<String>();
            var premises = new EqualityHashSet<String>();
            var compliance_rules = new EqualityHashSet<SimpleHornClause<string>>();
            // Printing the paths
            foreach (var x in ap.printAllPaths(starting_room, ending_room)) {
                resources.Clear();
                bool begin = true;
                string lastDoorBeforeCurrent = null;
                List<Pair<int, int>> doorList = new List<Pair<int, int>>();
                for (int i = 0; i < x.Count - 1; i++) {
                    var srcId = x[i];
                    var dstId = x[i + 1];
                    var cp = new Pair<int, int>(srcId, dstId);
                    foreach (var currKeyForDoor in rooms[srcId].resources_idest_keys) {
                        var key = keyForDoor(currKeyForDoor);
                        if (begin)
                            premises.Add(key);
                        else
                            resources.Add(key);
                    }
                    if (edgeMap.ContainsKey(cp)) {
                        var edge = edgeMap[cp];
                        if (edge.hasDoor) {
                            begin = false;
                            var door = doorName(cp);

                            // I can unlock the current dor only if I unlocked 
                            // all of the previous doors as wekll as I have the
                            // key for the current door
                            EqualityHashSet<String> tail = new EqualityHashSet<string>();
                            tail.Add(keyForDoor(cp));
                            foreach (var prevDoor in doorList) {
                                tail.Add(doorName(prevDoor));
                            }
                            compliance_rules.Add(new SimpleHornClause<string>(tail, door));

                            // Adding the current door to the list of the previous doors
                            doorList.Add(cp);

                            // The last door unlocked the possibility of accessing
                            // All of the resources encountered so far.
                            if (lastDoorBeforeCurrent != null) {
                                foreach (var res in resources) {
                                    compliance_rules.Add(new SimpleHornClause<string>(lastDoorBeforeCurrent, /*=>*/ res));
                                }
                            }
                            lastDoorBeforeCurrent = door;

                            resources.Clear();
                        }
                    } else {
                        Console.Error.WriteLine("ERROR: the edge does not exist!");
                        System.Environment.Exit(1);
                    }
                }
                if (!begin) {
                    candidate_end_door.Add(lastDoorBeforeCurrent);
                    foreach (var res in resources) {
                        compliance_rules.Add(new SimpleHornClause<string>(lastDoorBeforeCurrent, /*=>*/ res));
                    }
                }
            }
            if (compliance_rules.Count == 0) {
                return new Pair<bool, List<List<string>>>(true, new List<List<string>>());
            }
            var cwi = new ClosedWorldInference<string>(premises, compliance_rules);
            GenerateBacktrackStates<string> algorithm = new GenerateBacktrackStates<string>(cwi);
            var listOfStrategies = new List<List<string>>();
            foreach (var end in candidate_end_door) {
                var graph_list = algorithm.generateGraphs(new EqualityHashSet<string>(end));
                foreach (var graph in graph_list) {
                    if (graph.errors.Count == 0) {
                        List<string> ls = new List<string>();
                        var stack = graph.TopologicalSort();
                        foreach (int step in stack) {
                            var resOrDoors = graph.resolveNode(step).node;
                            if (resOrDoors.Count > 1) {
                                Console.Error.WriteLine("ERROR: Expected 1 door/resource!");
                                Environment.Exit(1);
                            } else {
                                ls.AddRange(resOrDoors);
                            }
                        } if (ls.Count > 0) {
                            listOfStrategies.Add(ls);
                        }
                    }
                }
            }
            return new Pair<bool, List<List<string>>>(listOfStrategies.Count>0, listOfStrategies);
        }

        public IEnumerable<int> outgoing(int d)
        {
            if (adj.Count > d)
                return adj[d];
            else
                return Enumerable.Empty<int>();
        }

        public void generate_data(double probability) {
            // Generating all of the possible rooms
            for (int i = 0; i < n_rooms; i++) {
                Room r;
                do {
                    r = generateRoom();
                } while (intersects(ref rooms, ref r));
                addRoom(ref r, i);
            }

            // Generating some edges for a sparse graph.
            for (int src = 0; src < n_rooms; src++) {
                for (int dst = 0; dst < src; dst++) {
                    double curr = rup.nextReal(ref rng);
                    if (curr <= probability)
                    {
                        Edge e = new Edge(rooms[src], rooms[dst]);
                        if (!intersects(ref corridors, ref e))
                        {
                            Console.WriteLine(src + "->" + dst);
                            adj[src].Add(dst);
                            adj[dst].Add(src);
                            addCorridor(src, dst, ref e);
                            addCorridor(dst, src, ref e);
                        }
                    }
                }
            }

            // Getting the starting and ending room for the dungeon
            var cp = maxDeg();
            var max = -1;
            starting_room = cp.key;
            for (int i = 0; i<n_rooms; i++)
            {
                if (cp.value[i]>max && (cp.value[i]<int.MaxValue))
                {
                    max = cp.value[i];
                    ending_room = i; 
                }
            }
        }
    }
}
