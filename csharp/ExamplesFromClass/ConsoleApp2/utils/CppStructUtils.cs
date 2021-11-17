using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.utils
{
    /// <summary>
    /// This class extends HashSets so to guarantee equality and hashcode generation as 
    /// expected in C++ code
    /// </summary>
    /// <typeparam name="H"></typeparam>
    public class EqualityHashSet<H> : HashSet<H> {
        public EqualityHashSet(){}
        public EqualityHashSet(int capacity) : base(capacity){}
        
        public EqualityHashSet(params H[] _objs) : this(_objs.Length) {
            foreach (var x in _objs) Add(x);
        }

        public EqualityHashSet(HashSet<H> _objs) : this(_objs.Count)  {
            foreach (var x in _objs) Add(x);
        }

        public EqualityHashSet(EqualityHashSet<H> _objs) : this(_objs.Count) {
            foreach (var x in _objs)  Add(x);
        }

        public override bool Equals(object obj) {
            EqualityHashSet<H> set = (EqualityHashSet<H>)obj;
            if (set == null) return false;
            return Count == set.Count && this.SetEquals(set);
        }

        public override int GetHashCode()
        {
            int i = 7;
            foreach (var x in new SortedSet<H>(this))
                i = i * 31 + x.GetHashCode();
            return i;
        }

        public override string ToString()
        {
            return "{"+string.Join(", ", this)+"}";
        }
    }

    public static class CppStructUtils
    {
        public static bool IsSubsetOf<K,V>(this Dictionary<K,V>.KeyCollection left, Dictionary<K, V>.KeyCollection right)
        {
            foreach (var x in left)
                if (!right.Contains(x)) return false;
            return true;
        }

        /// <summary>
        /// This function mimicks the operator[] for maps, where if the key is not in the map, then the map
        /// generates a new default value and associates it to the key, and then returns the newly created value,
        /// and otherwise returns the value associated in the key in the map
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="d"></param>
        /// <param name="toFind"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public static V GetOrInsert<K, V>(this Dictionary<K, V> d, K toFind, Func<V> generator) {
            if (d.ContainsKey(toFind))
                return d[toFind];
            else {
                var x = generator();
                d.Add(toFind, x);
                return x;
            }
        }

        public static HashSet<UInt64> GetOrInsert(this Dictionary<utils.Pair<UInt64, UInt64>, HashSet<UInt64>> d, utils.Pair<UInt64, UInt64> toFind)
        {
            if (d.ContainsKey(toFind))
                return d[toFind];
            else {
                var x = new HashSet<UInt64>();
                d.Add(toFind, x);
                return d[toFind];
            }
        }

        /// <summary>
        /// Mimicks the emplace method in C++, where new ojects are directly inserted within the data structure withouth requiring to explicitly call the constructor,
        /// thus making the code easier to read.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="list">Collection where to insert the element</param>
        /// <param name="parameters">Set of parameters required by the construtor to generate a new object</param>
        /// <returns></returns>
        public static ICollection<S> Emplace<S>(this ICollection<S> list, params object[] parameters)
        {
            list.Add((S)Activator.CreateInstance(typeof(S), parameters));
            return list;
        }

    }
}
