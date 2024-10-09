using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.grammar
{
    public static class GrammarMethods
    {

        public static bool ContainsSubsequence<T>(this IEnumerable<T> parent, IEnumerable<T> target)
        {
            var pattern = target.ToArray();
            var source = new LinkedList<T>();
            int from = 0;
            foreach (var element in parent) 
            {
                source.AddLast(element);
                if(source.Count == pattern.Length)
                {
                    if(source.SequenceEqual(pattern))
                        return true;
                    source.RemoveFirst();
                }

                from++;
            }
            return false;
        }
        
        public static List<T> replaceIthWith<T>(this List<T> list, int min, int max, List<T> with)
        {
            var al = new List<T>();
            var m = Math.Min(min, list.Count);
            for (var i = 0; i < m; i++)
            {
               al.Add(list[i]); 
            }
            foreach (var x in with)
                al.Add(x);
            var N = list.Count;
            for (var i=max+1; i<N; i++)
                al.Add(list[i]);
            return al;
        }
    }
}