using ConsoleApp2.utils;
using System;
using System.Diagnostics;

namespace ConsoleApp2.goap
{
    class GOAP {

        private static void preliminary_test()
        {
            {
                ClosedWorldInference<String> cwi1 = 
                    new ClosedWorldInference<String>(new EqualityHashSet<String>("A"), new EqualityHashSet<SimpleHornClause<String>>());
                Debug.Assert(cwi1.solvability_test(new EqualityHashSet<String>("A")));
            }
            {
                ClosedWorldInference<String> cwi1 = 
                    new ClosedWorldInference<String>(new EqualityHashSet<String>("A"), new EqualityHashSet<SimpleHornClause<String>>());
                Debug.Assert(!cwi1.solvability_test(new EqualityHashSet<String>("B")));
            }
            {
                
                ClosedWorldInference<String> cwi1 =
                    new ClosedWorldInference<String>(new EqualityHashSet<String>("A"), (EqualityHashSet < SimpleHornClause < String >> )new EqualityHashSet<SimpleHornClause<String>>().Emplace("C", "D"));
                Debug.Assert(!cwi1.solvability_test(new EqualityHashSet<String>("B")));
            }
            {
                ClosedWorldInference<String> cwi1 =
                    new ClosedWorldInference<String>(new EqualityHashSet<String>("A"), (EqualityHashSet<SimpleHornClause<String>>)new EqualityHashSet<SimpleHornClause<String>>().Emplace("A", "B"));
                Debug.Assert(!cwi1.solvability_test(new EqualityHashSet<String>("B")));
            }
        }

        public static void main()
        {
            if (false) {
                preliminary_test();
                LocksAndDoors lad = new LocksAndDoors();

                lad.generatePossibleStates("possible_states.dot");
                lad.generatePossibleStrategies(true, false).dot("one_single_strategy_correct");
                lad.generatePossibleStrategies(true, true).dot("one_single_strategy_with_error");
                lad.generatePossibleStrategies(false, false).dot("multiple_strategies_correct");

            }

            {
                StudyOrParty.main();
            }

        }
    }
}
