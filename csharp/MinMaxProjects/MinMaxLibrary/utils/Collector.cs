using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinMaxLibrary.utils
{
    public abstract class Collector<T, A, R>
    {

        //Func<T, Func<A>> supplier;
        //Func<T, Func<A, T, A>> accumulator;
        //Func<T, Func<A, A, A>> combiner;
        //Func<T, Func<A, R>> finisher;
        Func<A> ssupplier;
        Func<A, T, A> saccumulator;
        Func<A, A, A> scombiner;
        Func<A, R> sfinisher;
        bool addNewInList;
        List<A> acc;
        //Func<T, R> defaultValue;
        R def;
        T seed;

        public abstract Func<A> supplier(T seed);
        public abstract Func<A, T, A> accumulator(T seed);
        public abstract Func<A, A, A> combiner(T seed);
        public abstract Func<A, R> finisher(T seed);
        public abstract R defaultValue(T seed);

        public Collector() {
            acc = new List<A>();
            addNewInList = true;
        }

        protected Collector<T, A, R> setSeed(T x)
        {
            this.seed = x;
            ssupplier = supplier(x);
            saccumulator = accumulator(x);
            scombiner = combiner(x);
            sfinisher = finisher(x);
            def = defaultValue(x);
            return this;
        }

        public abstract Collector<T, A, R> generateNew(T seed);

        public void append(T x)
        {
            if ((acc.Count == 0) || (addNewInList)) {
                acc.Add(ssupplier());
            }
            acc[acc.Count - 1] = saccumulator(acc[acc.Count - 1], x);
        }

        public void init(T x)
        {
            acc.Add(ssupplier());
            acc[0] = saccumulator(acc[0], x);
        }

        public void append(A x)
        {
            acc.Add(x);
            addNewInList = true;
        }

        public Optional<A> totalAccumulate()
        {
            if (acc.Count == 0)
                return new Optional<A>();
            else
            {
                A accX = acc[0];
                for (int i = 1; i < acc.Count; i++)
                    accX = scombiner(accX, acc[i]);
                acc.Clear();
                acc.Add(accX);
                return new Optional<A>(accX);
            }
        }

        public R collectResult(Optional<A> val)
        {
            if (val.HasValue)
                return sfinisher(val.Value);
            else
                return def;
        }

        public R collectResult()
        {
            if (acc.Count == 0)
                return def;
            else
            {
                A accX = acc[0];
                for (int i = 1; i < acc.Count; i++)
                    accX = scombiner(accX, acc[i]);
                acc.Clear();
                return sfinisher(accX);
            }
        }

    }
}
