using System;
using System.Collections.Generic;
using ConsoleApp2.utils;

namespace ConsoleApp2.genetic.chromosomes
{
    public class MutatedChromosome : IChromosome
    {
        private IChromosome orig;
        private readonly int _ofPopulation;
        private readonly double _probMutation;
        private int os;

        public MutatedChromosome(IChromosome orig, int ofPopulation, double probMutation) {
            this.orig = orig;
            _ofPopulation = ofPopulation;
            _probMutation = probMutation;
            this.os = orig.size();
        }

        public int size() {
            return os;
        }

        public double value(int pos, ulong rng) {
            ulong idx = (ulong)_ofPopulation * (ulong)os + (ulong)pos;
            return RanNumericalRecipes.doub(idx) < _probMutation ? 
                    orig.value(pos, rng) : 
                    RanNumericalRecipes.doub(idx ^ rng);
        }

        public double[] GetArray(ulong rng_xoroff)
        {
            return new ConcreteChromosome(this, rng_xoroff).GetArray(0);
        }
    }
}