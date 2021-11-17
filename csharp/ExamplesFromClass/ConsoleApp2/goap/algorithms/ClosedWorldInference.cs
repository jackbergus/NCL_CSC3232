using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.goap
{
    public class ClosedWorldInference<T> {
        public Dictionary<T, double> init { get;  }
        public Dictionary<SimpleHornClause<T>, double> rules { get;  }

        public ClosedWorldInference(EqualityHashSet<T> init, EqualityHashSet<SimpleHornClause<T>> rules)
        {
            this.init = new Dictionary<T, double>();
            foreach (var x in init)
                this.init[x] = 1.0;
            this.rules = new Dictionary<SimpleHornClause<T>, double>();
            foreach (var x in rules)
                this.rules[x] = 1.0;
        }

        public ClosedWorldInference(Dictionary<T, double> init, Dictionary<SimpleHornClause<T>, double> rules)
        {
            this.init = init;
            this.rules = rules;
        }

        public EqualityHashSet<T> grounding(bool debug = true)
        {
            EqualityHashSet<T> S = null;
            EqualityHashSet<T> tmp = new EqualityHashSet<T>();
            tmp.UnionWith(init.Keys); // For non-dictionary, just remove Keys
            do
            {
                S = new EqualityHashSet<T>(tmp);
                if (debug) Console.WriteLine(S.ToString() + " vs. " + tmp.ToString());
                foreach (var cp in rules.Keys) // For non-dictionary, just remove Keys 
                {
                    if (debug) Console.WriteLine("Testing rule: " + cp.ToString());
                    if (cp.tail.IsSubsetOf(tmp) && (!tmp.Contains(cp.head))) {
                        if (debug) Console.WriteLine("- Applying rule: " + cp.ToString());
                        tmp.Add(cp.head);
                        if (debug) Console.WriteLine(" * State becomes: {" + string.Join(",", tmp.Select(i => i.ToString())) + "}");
                    }
                }
            } while ((!S.IsSubsetOf(tmp)) || (!tmp.IsSubsetOf(S)));
            return tmp;
        }

        public bool solvability_test(EqualityHashSet<T> goal, bool debug = true)
        {
            if (goal.IsSubsetOf(init.Keys))
                return true;
            else
            {
                return (goal.IsSubsetOf(grounding(debug)));
            }
        }





        

    }
}
