using ConsoleApp2.utils;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.goap.structures
{
    class EqualityHashSetWithMultipleWeights<T> {
        Dictionary<T, double> weights { get; }
        public EqualityHashSet<T> elements { get; }

        public List<double> projectCosts(EqualityHashSet<T> x) {
            return x.Where(i => elements.Contains(i)).Select(i => weights[i]).ToList();
        }

        public void Add(T x, double w)
        {
            if (!elements.Contains(x))
            {
                elements.Add(x);
            }
            weights[x] = w;
        }

        public EqualityHashSetWithMultipleWeights(EqualityHashSetWithMultipleWeights<T> x)
        {
            weights = new Dictionary<T, double>(x.weights);
            elements = new EqualityHashSet<T>(x.elements);
        }

        public EqualityHashSetWithMultipleWeights(Dictionary<T, double> weights)
        {
            this.weights = weights;
            elements = new EqualityHashSet<T>(weights.Keys.ToArray());
        }



        public EqualityHashSetWithMultipleWeights(IEnumerable<T> weights, double defWeight = 1.0)
        {
            this.weights = new Dictionary<T, double>();
            elements = new EqualityHashSet<T>(this.weights.Count);
            foreach (var x in weights)
            {
                elements.Add(x);
                this.weights[x] = defWeight;
            }
        }

        public void mergeWith(EqualityHashSetWithMultipleWeights<T> s)
        {
            foreach (var x in s.weights)
                    Add(x.Key, x.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is EqualityHashSetWithMultipleWeights<T> weights &&
                   this.elements.Equals(weights.elements);
        }

        public override int GetHashCode()
        {
            return elements.GetHashCode();
        }
    }
}
