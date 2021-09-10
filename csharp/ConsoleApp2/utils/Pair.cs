using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.utils
{
    public class Pair<TKey, TValue> : IEquatable<Pair<TKey, TValue>>
    {
        public TKey key;
        public TValue value;

        public Pair(TKey key, TValue value) {
            this.key = key;
            this.value = value;
        }

        public bool Equals(Pair<TKey, TValue> y)
        {
            return key.Equals(y.key) && value.Equals(y.value);
        }

        public override bool Equals(object obj)
        {
            return obj is Pair<TKey, TValue> && Equals((Pair<TKey, TValue>)obj);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode() ^ value.GetHashCode(); // Or something like that
        }

    }
}
