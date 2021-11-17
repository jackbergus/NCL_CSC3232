using ConsoleApp2.goap;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.utils
{
    static class Operations
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences) {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));
        }

        public static void dot<K,V>(this IEnumerable<WeightedMultiGraph<K, V>> collection, string filename_base)
        {
           int i = 0;
           foreach (var graph in collection)  {
                graph.dot(filename_base + "_" + i.ToString() + ".dot");
                i++;
            }
        }

        public static EqualityHashSet<T> ToEqualityHashSet<T>(this IEnumerable<T> x)
        {
            EqualityHashSet<T> res = new EqualityHashSet<T>();
            foreach (var y in x)
                res.Add(y);
            return res;
        }
    }
}
