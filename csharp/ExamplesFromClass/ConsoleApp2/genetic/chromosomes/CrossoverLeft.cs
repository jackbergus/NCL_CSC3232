using System;
using ConsoleApp2.utils;

namespace ConsoleApp2.genetic.chromosomes
{
    public class CrossoverLeft : IChromosome
    {
        
        private IChromosome left, right;
        private bool isMinLeft;
        private bool isMinRight;
        private readonly bool _leftGene;
        private int os;
        private readonly int idxActual;

        public CrossoverLeft(IChromosome left, 
                             IChromosome right, 
                             int ofPopulationL,
                             int ofPopulationR,
                             bool leftGene)
        {
            this.left = left;
            this.right = right;
            _leftGene = leftGene;
            this.os = Math.Max(left.size(), right.size());
            isMinLeft = left.size() < right.size();
            isMinRight = right.size() < left.size();
            ulong idx = (ulong)(ofPopulationL ^ ofPopulationR);
            idxActual = (int)(RanNumericalRecipes.doub(idx) * os);
        }
        
        public int size() {
            return os;
        }

        public double value(int pos, ulong rng_xoroff)
        {
            if (isMinLeft)
                return right.value(pos, rng_xoroff);
            if (isMinRight)
                return left.value(pos, rng_xoroff);
            return (pos < idxActual && _leftGene) ? 
                left.value(pos, rng_xoroff) : 
                right.value(pos, rng_xoroff);
        }

        public double[] GetArray(ulong rng_xoroff)
        {
            return new ConcreteChromosome(this, rng_xoroff).GetArray(0);
        }
    }
}