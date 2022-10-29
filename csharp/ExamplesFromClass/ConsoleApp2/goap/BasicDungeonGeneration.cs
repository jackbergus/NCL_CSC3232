using System;
namespace ConsoleApp2.goap {
    public class BasicDungeonGeneration {
        public static void testing() {
            // setting up the basic parameters for the dungeon generation
            ProceduralDungeon dungeon = new ProceduralDungeon(5, 2, 2, 50, 50);

            // Density should be between 0.0 and 1.0.
            // A denser graph will result in a potentially fully connected 
            // level. This generates non-overlapping rooms as well as non-
            // intersecting paths
            double density = 1.0;
            dungeon.generate_data(density);


            int finalDoor = -1;
            foreach (int src in dungeon.outgoing(dungeon.ending_room)) {
                finalDoor = src;
                // By commenting the following line, the game is no more solvable!
                dungeon.addKeyForDoorInRoom(dungeon.starting_room, src, dungeon.ending_room);
                break;
            }
            // Adding a door in one of the edges leading to the final room 
            dungeon.setLockedDoorOnEdge(finalDoor, dungeon.ending_room);
            // No Unity project! This should be the part of your work :)
            dungeon.printOnTerminal();

            // Checking whether the current dungeon is feasable
            var x = dungeon.isFeasable();
            if (x.key) {
                for (int i = 0; i < x.value.Count; i++) {
                    // Printing each possible strategy for solving the game!
                    Console.WriteLine("Strategy #" + i + ":");
                    Console.Write(" * ");
                    foreach (var y in x.value[i])
                        Console.Write(y + ", "); // Printing each step of the strategy
                    Console.WriteLine("");

                    // Any strategy that is here a sequence can be used as follows:
                    // - Either as a sequence of instructions that an AI should follow in order to accomplish the level
                    // - Or to provide positive feedback to the player: after t seconds, if the player has not completed some of the first actions, I can provide a suggestion to the next action to perform
                    // - All of these strategies might be ranked, and the best one can be picked (e.g., maximum distance associated between the nodes)
                    //   E.g., I might also simplify the level by forcing the player into one of the possible strategies
                }
            } else {
                // it's time to take out the trash!
                Console.WriteLine("the game is not feasable!");
            }
        }
    }
}
