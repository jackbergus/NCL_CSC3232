namespace ConsoleApp2.probabilities
{
    /// <summary>Class <c>Door</c> models a door in a given state, ideally leading towards another given door.</summary>
    public class Door
    {
        public bool locked;
        public bool wrong_door;
        public bool is_choosen { get; set; }
        public long reachable_state;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locked">Whether the door is locked or not</param>
        /// <param name="wrongDoor">Whether the door is the expected/correct one for the solution</param>
        /// <param name="reachableState">If the door is not locked, this determines the leading state</param>
        public Door(bool locked = true, bool wrongDoor = true, long reachableState = -1)
        {
            this.locked = locked;
            wrong_door = wrongDoor;
            is_choosen = false;
            reachable_state = reachableState;

        }

        /*public Door()  [in C#10] : this(true, true, -1) {
            
        }*/
    }
}
