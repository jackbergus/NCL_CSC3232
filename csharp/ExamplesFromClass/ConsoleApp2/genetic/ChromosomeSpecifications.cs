using System;
using System.Collections.Generic;
using System.Linq;
using Alba.CsConsoleFormat;
using ConsoleApp2.genetic.chromosomes;
using ConsoleApp2.utils;

namespace ConsoleApp2.genetic
{
    public class ChromosomeSpecifications<Obs, Act>
    {
        private DiscreteObservation<Obs>[] ls;
        private string[] observation_names;
        private int[] indexing;
        private DiscreteAction<Act> actions;
        private Random randNum;
        private int _genomeSize;

        public ChromosomeSpecifications(Dictionary<String, DiscreteObservation<Obs>> d, 
                          DiscreteAction<Act> actions,
                          int seed)
        {
            observation_names = new string[d.Count];
            this.ls = new DiscreteObservation<Obs>[d.Count];
            int j = 0;
            foreach (var x in d)
            {
                observation_names[j] = x.Key;
                this.ls[j] = x.Value;
                j++;
            }
            this.indexing = new int[d.Count];
            _genomeSize = actions.size();
            if (this.ls.Length >0)  {
                this.indexing[ls.Length - 1] = _genomeSize; // Here: actions' size
                for (int i = 2; i <= ls.Length; i++) {
                    this.indexing[ls.Length - i] = this.indexing[ls.Length - i + 1] * ls[ls.Length - i +1].size();
                }
            }
            this.actions = actions ?? throw new ArgumentNullException(nameof(actions));
            this.randNum = new Random(seed);
            foreach (var x in ls)
                _genomeSize *= x.size();
        }
        
        // public double[] Mutate(double[] genome, 
        //                      double probability)
        // {
        //     string ret = "";
        //     double randomVariable = 0.0;
        //     double[] mutatedGenome = new double[genome.Length];
        //     for (int i = 0; i < genome.Length; i++) {
        //         randomVariable = randNum.NextDouble();
        //         mutatedGenome[i] = (randomVariable < probability) ? randNum.NextDouble() : genome[i];
        //     }
        //     return mutatedGenome;
        // }

        // public void printGenome(IChromosome chromosome,  ulong rng_xoroff)
        // {
        //     var g = new Grid { Stroke = StrokeHeader, StrokeColor = DarkGray}
        //         .AddColumns(observation_names.Select(x => new Column(){ Width = GridLength.Auto }).ToArray(), new Column(){ Width = GridLength.Auto }, new Column(){ Width = GridLength.Auto })
        //         .AddChildren(observation_names.Select(x => new Cell { Stroke = StrokeHeader, Color = White }.AddChildren(x)), "Action", "Weight")
        //     ;
        // }

        public Pair<int,int> MinMaxVectorOffset(Obs[] ls) {
            if ((ls is null) || (ls.Length == 0))
            if (ls.Length != this.ls.Length) return null;
            int res = 0;
            for (int i = 0; i < this.ls.Length; i++)
                res += this.ls[i].getId(ls[i]) * this.indexing[i];
            return new Pair<int, int>(res, res+actions.size()-1);
        }

        public int genomeSize() {
            return _genomeSize;
        }

        public IEnumerable<Pair<Act,double>> getTopNActions(double[] anArray, 
                                                            Pair<int, int> cp,
                                                            int n)
        {
            int copy = n;
            int acts = actions.size();
            return anArray.Select((value, index) => new { value, index })
                .Where((arg => arg.index >= cp.key && arg.index <= cp.value))
                .OrderByDescending(vi => vi.value)
                .Select(arg => new Pair<Act,double>(actions.fromId(arg.index % acts), arg.value))
        .TakeWhile(
                    arg =>
                    {
                        copy -= 1;
                        return copy > 0;
                    });
        }
        
        public IEnumerable<Pair<Act,double>> getBotNActions(double[] anArray, 
            Pair<int, int> cp,
            int n)
        {
            int copy = n;
            int acts = actions.size();
            return anArray.Select((value, index) => new { value, index })
                .Where((arg => arg.index >= cp.key && arg.index <= cp.value))
                .OrderBy(vi => vi.value)
                .Select(arg => new Pair<Act,double>(actions.fromId(arg.index % acts), arg.value))
                .TakeWhile(
                    arg =>
                    {
                        copy -= 1;
                        return copy > 0;
                    });
        }

        public IChromosome generateRandomGenome() {
            double[] genome = new double[genomeSize()];
            for (int i = 0; i < genome.Length; i++)
                genome[i] = randNum.NextDouble();
            return new ConcreteChromosome(genome);
        }
    }
}