using System;
namespace MinMaxLibrary.utils
{
    public class Accumulator<T>
    {
        T bCase;
        Func<T, T, T> acc;

        public Accumulator(T bCase, Func<T, T, T> acc)
        {
            this.bCase = bCase;
            this.acc = acc;
        }

        public void accumulate(T arg)
        {
            bCase = acc(bCase, arg);
        }

        public T get()
        {
            return bCase;
        }
    }
}
