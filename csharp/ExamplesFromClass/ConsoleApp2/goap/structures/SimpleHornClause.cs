using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.goap
{
    public class SimpleHornClause<T> {
        public EqualityHashSet<T> tail;
        public T head;

        public SimpleHornClause(params T[] _vs)
        {
            tail = new EqualityHashSet<T>();
            int i = 0, N = _vs.Length;
            for (; i<N-1; i++)
            {
                tail.Add(_vs[i]);
            }
            if (N > 0)
            {
                head = _vs[N - 1];
            }
        }

        public SimpleHornClause(EqualityHashSet<T> tail, T head)
        {
            this.tail = tail;
            this.head = head;
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleHornClause<T> clause &&
                   EqualityComparer<EqualityHashSet<T>>.Default.Equals(tail, clause.tail) &&
                   EqualityComparer<T>.Default.Equals(head, clause.head);
        }

        public override int GetHashCode()
        {
            int hashCode = 2137858718;
            hashCode = hashCode * -1521134295 + EqualityComparer<EqualityHashSet<T>>.Default.GetHashCode(tail);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(head);
            return hashCode;
        }

        public override String ToString()
        {
            return string.Join(" and ", tail.Select(i => i.ToString())) + " => " + head.ToString();
        }
    }
}
