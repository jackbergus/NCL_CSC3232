using ConsoleApp2.goap.algorithms;
using System;

namespace ConsoleApp2.goap
{
    class StudyOrParty
    {
        public static void main()
        {

            WeightedMultiGraph<string, string> G = new WeightedMultiGraph<string, string>();
            var reading_1 = G.addVertex("ReadingDay1", true);
            var reading_2 = G.addVertex("ReadingDay2");
            var reading_3 = G.addVertex("ReadingDay3");
            var party_1 = G.addVertex("Party1");
            var party_2 = G.addVertex("Party2");
            var party_3 = G.addVertex("Party3");
            var passed = G.addVertex("PassedExam", false, true);
            G.addEdge(reading_1, reading_2, "study", 0.7, -2.0);
            G.addEdge(reading_1, party_1, "party!", 0.3, 1.0);
            G.addEdge(party_1, reading_1, "headache", 1.0, -2.0);

            G.addEdge(reading_2, reading_3, "study", 0.8, -2.0);
            G.addEdge(reading_2, party_2, "party!", 0.2, 1.0);
            G.addEdge(party_2, reading_2, "headache", 0.8, -1.0);
            G.addEdge(party_2, party_1, "strong headache", 0.2, -1.0);


            G.addEdge(reading_3, passed, "study&pass", 0.9, 10.0);
            G.addEdge(reading_3, party_3, "party!", 0.1, 1.0);
            G.addEdge(party_3, reading_3, "headache", 0.8, -1.0);
            G.addEdge(party_3, party_2, "strong headache", 0.2, -1.0);

            PolicyIteration<string, string> policyIteration = new PolicyIteration<string, string>(G, 1.0);
            policyIteration.loop(0.01);

            foreach (var cp in policyIteration.V)
                Console.WriteLine(" Value(" + cp.Key.ToString() + ")=" + cp.Value.ToString());

            foreach (var cp in policyIteration.detPolicy)
                if (cp.Value != null)
                    Console.WriteLine(" Pi("+cp.Key.ToString() + ")="+cp.Value.ToString());


            foreach (var cp in policyIteration.policy)
                foreach (var cp2 in cp.Value)
                    if (cp2.Key != null)
                         Console.WriteLine(" Pi(" + cp.Key.ToString() + "|" + cp2.Key.ToString() +")=" + cp2.Value.ToString());
        }
    }
}
