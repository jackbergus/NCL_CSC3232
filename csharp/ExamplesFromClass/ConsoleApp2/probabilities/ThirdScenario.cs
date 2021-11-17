using ConsoleApp2.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp2.probabilities
{
    /// <summary>
    /// The Third scenario differs from the second by just the implementation of a simple strategy (the choice of remembering the previous choice)
    /// </summary>
    public class ThirdScenario {
        Random generator_s1;        
        Random generator_s2;
        Random generator_s3;
        HashSet<utils.Pair<UInt64, UInt64>>[] s1_position_to_other_choices, 
                                              s2_position_to_other_choices, 
                                              s3_position_to_other_choices;
        Dictionary<utils.Pair<UInt64, UInt64>, HashSet<UInt64>> s1_s2_position_to_other_choices,
                                                                s1_s3_position_to_other_choices,
                                                                s2_s3_position_to_other_choices;
        HashSet<utils.Pair<UInt64, utils.Pair<UInt64, UInt64>>> s1_s2_s3_attempts;
        utils.UniformRandom door_distr;
        Board default_board;
        SecondScenario sn;
        int doors;

        public ThirdScenario(int seed) {
            sn = new SecondScenario(4, 6);
            door_distr = new utils.UniformRandom(0, 5);
            generator_s1 = new Random(seed+0);
            generator_s2 = new Random(seed+1);
            generator_s3 = new Random(seed+2);
            this.doors = (int)6;
        }

        void setSeed(int seed) {
            generator_s1 = generator_s2 = generator_s3 = null;
            generator_s1 = new Random(seed + 0);
            generator_s2 = new Random(seed + 1);
            generator_s3 = new Random(seed + 2);
        }

        void clearAll()
        {
            if (s1_position_to_other_choices != null) 
                foreach (var x in s1_position_to_other_choices) x.Clear();
            else
                s1_position_to_other_choices = Enumerable.Range(0, (int)doors).Select(i => new HashSet<utils.Pair<UInt64, UInt64>>()).ToArray();
            
            if (s2_position_to_other_choices != null) 
                foreach (var x in s2_position_to_other_choices) x.Clear();
            else
                s2_position_to_other_choices = Enumerable.Range(0, (int)doors).Select(i => new HashSet<utils.Pair<UInt64, UInt64>>()).ToArray();
           
            if (s3_position_to_other_choices != null) 
                foreach (var x in s3_position_to_other_choices) x.Clear();
            else
                s3_position_to_other_choices = Enumerable.Range(0, (int)doors).Select(i => new HashSet<utils.Pair<UInt64, UInt64>>()).ToArray();
            
            if (s1_s2_position_to_other_choices != null) 
                s1_s2_position_to_other_choices.Clear();
            else
                s1_s2_position_to_other_choices = new Dictionary<utils.Pair<UInt64, UInt64>, HashSet<UInt64>>();
            
            if (s1_s3_position_to_other_choices != null) 
                s1_s3_position_to_other_choices.Clear();
            else
                s1_s3_position_to_other_choices = new Dictionary<utils.Pair<UInt64, UInt64>, HashSet<UInt64>>();
            
            if (s2_s3_position_to_other_choices != null) 
                s2_s3_position_to_other_choices.Clear();
            else
                s2_s3_position_to_other_choices = new Dictionary<utils.Pair<UInt64, UInt64>, HashSet<UInt64>>();
            
            if (s1_s2_s3_attempts != null) 
                s1_s2_s3_attempts.Clear();
            else
                s1_s2_s3_attempts = new HashSet<utils.Pair<UInt64, utils.Pair<UInt64, UInt64>>>();
        }

        private static utils.Pair<UInt64, UInt64> genPair(int i, int j)
        {
            return new utils.Pair<UInt64, UInt64>((uint)i, (uint)j);
        }


        private static utils.Pair<UInt64, utils.Pair<UInt64, UInt64>> genTriple(int i, int j, int k)
        {
            return new utils.Pair<UInt64, utils.Pair<UInt64, UInt64>>((uint)i, new utils.Pair<UInt64, UInt64>((uint)j, (uint)k));
        }

        public ulong testRandomCaseScenario(bool debugInfo = false)
        {
            default_board = sn.initScenario();
            clearAll();
            // Counting the attempts
            ulong attempts = 0;

            bool not_found = true;
            while (not_found)
            {
                int i, j, k;
                ulong tryouts = 0;
                do
                {
                    tryouts++;
                    i = door_distr.nextInt(ref generator_s1);
                    if (debugInfo && (tryouts == 1)) Console.WriteLine("i={0}", i);
                    if (debugInfo && (tryouts > 1)) Console.WriteLine("changing i to {0}", i);
                } while (s1_position_to_other_choices[i].Count == 36);

                tryouts = 0;
                do
                {
                    tryouts++;
                    j = door_distr.nextInt(ref generator_s2);
                    if (debugInfo && (tryouts == 1)) Console.WriteLine("i,j={0},{1}", i, j);
                    if (debugInfo && (tryouts > 1)) Console.WriteLine("i={0}, changing j to {0}", i,j);
                } while ((s2_position_to_other_choices[j].Count == 36) ||
                         (s1_s2_position_to_other_choices.GetOrInsert(genPair(i, j)).Count == 6));

                tryouts = 0;
                do
                {
                    tryouts++;
                    k = door_distr.nextInt(ref generator_s3);
                    if (debugInfo && (tryouts == 1)) Console.WriteLine("i,j,k={0}", i,j,k);
                    if (debugInfo && (tryouts > 1)) Console.WriteLine("i,j = {0},{1}; changing k to {0}", i, j, k);
                } while ((s1_s2_s3_attempts.Contains(genTriple(i, j, k))) ||
                            (s3_position_to_other_choices[k].Count == 36) ||
                            (s1_s3_position_to_other_choices.GetOrInsert( genPair(i, k)).Count == 6) ||
                            (s2_s3_position_to_other_choices.GetOrInsert(genPair(j, k)).Count == 6));



                
                if (default_board.transition_from_states[0].ElementAt((int)i).wrong_door ||
                    default_board.transition_from_states[1].ElementAt((int)j).wrong_door ||
                    default_board.transition_from_states[2].ElementAt((int)k).wrong_door) {
                    if (debugInfo)
                        Console.WriteLine("Wrong configuration: {0} {1} {2}!", i, j, k);
                    s1_position_to_other_choices[i].Add(genPair(j, k));
                    s1_position_to_other_choices[j].Add(genPair(i, k));
                    s1_position_to_other_choices[k].Add(genPair(i, j));

                    s1_s2_position_to_other_choices.GetOrInsert(genPair(i, j)).Add((uint)k);
                    s1_s3_position_to_other_choices.GetOrInsert(genPair(i, k)).Add((uint)j);
                    s2_s3_position_to_other_choices.GetOrInsert(genPair(j, k)).Add((uint)i);

                    s1_s2_s3_attempts.Add(genTriple(i, j, k));
                }
                else
                {
                    if (debugInfo)
                        Console.WriteLine("Winning configuration: {0} {1} {2}!", i, j, k);
                    not_found = false;
                }
                attempts++;
            }

            if (debugInfo) {
                Console.WriteLine("Attempts: {0}", attempts+1);
                Console.WriteLine("");
                Console.WriteLine("");
            }
            Debug.Assert(attempts <= 216, "There should be at most 216 attempts: we got " + (attempts).ToString());
            return (attempts + 1);

        }
    }
}
