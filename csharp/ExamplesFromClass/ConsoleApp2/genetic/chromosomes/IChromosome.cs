using System;

namespace ConsoleApp2.genetic.chromosomes
{
    public interface IChromosome
    {
        int size();
        double value(int pos, ulong rng_xoroff);
        double[] GetArray(ulong rng_xoroff);
    }
}