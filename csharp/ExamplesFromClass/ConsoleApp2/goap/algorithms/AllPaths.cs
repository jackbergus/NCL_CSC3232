using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.goap.algorithms
{
    public class AllPaths
    {

        List<List<int>> allPaths;
        List<HashSet<int>>adj;

        public AllPaths(ref List<HashSet<int>> adj)
        {
            allPaths = new List<List<int>>();
            this.adj = adj;
        }

        public List<List<int>> printAllPaths(int s, int d)
        {
            bool[] isVisited = new bool[adj.Count];
            List<int> pathList = new List<int>();

            // add source to path[]
            pathList.Add(s);

            // Call recursive utility
            printAllPathsUtil(s, d, isVisited, pathList);

            return allPaths;
        }

        // A recursive function to print
        // all paths from 'u' to 'd'.
        // isVisited[] keeps track of
        // vertices in current path.
        // localPathList<> stores actual
        // vertices in the current path
        private void printAllPathsUtil(int u, int d,
                                       bool[] isVisited,
                                       List<int> localPathList)
        {

            if (u.Equals(d))
            {
                allPaths.Add(localPathList.Select(x => x).ToList());
                // if match found then no need
                // to traverse more till depth
                return;
            }

            // Mark the current node
            isVisited[u] = true;

            // Recur for all the vertices
            // adjacent to current vertex
            foreach (int i in adj[u])
            {
                if (!isVisited[i])
                {
                    // store current node
                    // in path[]
                    localPathList.Add(i);
                    printAllPathsUtil(i, d, isVisited,
                                      localPathList);

                    // remove current node
                    // in path[]
                    localPathList.Remove(i);
                }
            }

            // Mark the current node
            isVisited[u] = false;
        }
    }
}
