using ConsoleApp2.goap.algorithms;
using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApp2.goap
{
    public class LocksAndDoors  {
        static string key_a = "Key_A";
        static string key_b = "Key_B";
        static string key_c = "Key_C";
        static string key_d = "Key_D";
        static string key_e = "Key_E";
        static string key_f = "Key_Finish";
        static string key_t = "Key_T";
        static string door_a = "Door_A";
        static string door_b = "Door_B";
        static string door_ce = "Door_CE";
        static string door_d1 = "Door_D1";
        static string door_d2 = "Door_D2";
        static string door_t = "Door_T";
        static string door_f = "Door_Finish";
        static SimpleHornClause<string> r1 = new SimpleHornClause<string>(key_a /* => */, door_a);
        static SimpleHornClause<string> r2a = new SimpleHornClause<string>(door_a /* => */, key_e);
        static SimpleHornClause<string> r2b = new SimpleHornClause<string>(door_a /* => */, key_d);
        static SimpleHornClause<string> r3a = new SimpleHornClause<string>(door_a, key_d  /* => */, door_d2);
        static SimpleHornClause<string> r3b = new SimpleHornClause<string>(key_d /* => */, door_d1);
        static SimpleHornClause<string> r4 = new SimpleHornClause<string>(door_a, key_t /* => */, door_t);
        static SimpleHornClause<string> r5 = new SimpleHornClause<string>(door_d2 /* => */, key_t);
        static SimpleHornClause<string> r6 = new SimpleHornClause<string>(door_t /* => */, key_c);
        static SimpleHornClause<string> r7 = new SimpleHornClause<string>(door_d1 /* => */, key_b);
        static SimpleHornClause<string> r8 = new SimpleHornClause<string>(door_d1, key_b /* => */, door_b);
        static SimpleHornClause<string> r9 = new SimpleHornClause<string>(door_b /* => */, key_f);
        static SimpleHornClause<string> r10 = new SimpleHornClause<string>(key_c, key_e /* => */, door_ce);
        static SimpleHornClause<string> r11 = new SimpleHornClause<string>(door_ce, key_f /* => */, door_f);

        public LocksAndDoors(bool test = false)
        {
            if (test) {
                // Testing that everything is working fine
                ClosedWorldInference<string> cwi = new ClosedWorldInference<string>(new EqualityHashSet<string>(key_a), 
                                                                                    new EqualityHashSet<SimpleHornClause<string>>(r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11));
                cwi.solvability_test(new EqualityHashSet<string>(door_f), true);
            }
        }

        public void generatePossibleStates(string dot_filename) {
            /// Setting up the world over which we are going to perform the inference
            ClosedWorldInference<string> cwi = new ClosedWorldInference<string>(new EqualityHashSet<string>(key_a),
                                                                                new EqualityHashSet<SimpleHornClause<string>>(r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11));
            /// Creating an instance of the algorithm over the world to be inferred
            GenerateUnweightedPossibleStates<String> algorithm = new GenerateUnweightedPossibleStates<String>(cwi);
            /// Generating the graph and printing it to the output
            algorithm.generateUnweightedPossibleStates(new EqualityHashSet<string>(door_f)).dot(dot_filename);  
        }

        public List<WeightedMultiGraph<EqualityHashSet<string>, SimpleHornClause<string>>> generatePossibleStrategies(bool OneSinglePossibleStrategy, bool OneSingleStrategyWithErrors) {
            ClosedWorldInference<string> cwi = null;
            if (OneSinglePossibleStrategy)
            {
                if (!OneSingleStrategyWithErrors)
                    cwi = new ClosedWorldInference<string>(new EqualityHashSet<string>(key_a),
                                                                                new EqualityHashSet<SimpleHornClause<string>>(r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11));
                else
                    cwi = new ClosedWorldInference<string>(new EqualityHashSet<string>(key_a),
                                                                                new EqualityHashSet<SimpleHornClause<string>>(r1, r2a, r2b, r3a, r3b, r4, r5, r6, r8, r9, r10, r11));
            }
            else
            {

                SimpleHornClause<string> r6b = new SimpleHornClause<string>(door_t /* => */, key_f);
                SimpleHornClause<string> r9b = new SimpleHornClause<string>(door_b /* => */, key_c);
                SimpleHornClause<string> r11b = new SimpleHornClause<string>(door_a, door_t, key_f /* => */, door_f);
                SimpleHornClause<string> r11c = new SimpleHornClause<string>(door_d1, door_b, key_f /* => */, door_f);
                cwi = new ClosedWorldInference<string>(new EqualityHashSet<string>(key_a),
                                                                            new EqualityHashSet<SimpleHornClause<string>>(r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11, r6b, r9b, r11b, r11c));
            }
            GenerateBacktrackStates<string> algorithm = new GenerateBacktrackStates<string>(cwi);
            var x = algorithm.generateGraphs(new EqualityHashSet<string>(door_f));
            if (OneSinglePossibleStrategy)
            {
                Debug.Assert(x.Count == 1);
            }
            return x;
        }

    }
}
