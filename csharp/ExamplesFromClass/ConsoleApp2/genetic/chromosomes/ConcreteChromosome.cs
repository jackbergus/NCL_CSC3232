using System;

namespace ConsoleApp2.genetic.chromosomes
{
    public class ConcreteChromosome : IChromosome
    {
        private double[] array;
        public ConcreteChromosome(double[] array) {
            this.array = array;
        }

        public ConcreteChromosome(IChromosome any, ulong rng_xoff)
        {
            array = new double[any.size()];
            for (int i = 0; i < array.Length; i++) array[i] = any.value(i, rng_xoff);
        }

        public double[] GetArray(ulong rng_xoroff)
        {
            return array; 
        }

        public int size()  {
            return array.Length;
        }

        public double value(int pos, ulong rng) {
            return array[pos]; // In a concrete chromosome, random has no effect!
        }
    }
}