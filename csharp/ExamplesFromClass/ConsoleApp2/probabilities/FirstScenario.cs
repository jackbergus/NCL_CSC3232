namespace ConsoleApp2.probabilities
{
    class FirstScenario : BasicScenarios
    {
        class OnWrongDoNothing : SimpleNavigationFunction
        {
            public bool onWrongDoorDo(ref ulong x, ref Board b, ref Door d) { return true; }

            public void onWrongStateDo(ref ulong currState, ref Board b)
            {
                // noop
            }
        }

        public FirstScenario(ulong n = 4, ulong doors = 6) : base(n, doors) { }

        public override Board initScenario() {
            Board board = new Board(nStates, nDoors);
            long s = 0;
            foreach (Door[] adj in board.transition_from_states)
            {
                if (adj.Length > 0)
                {
                    ref Door x = ref adj[adj.Length - 1];
                    x.locked = false;
                    x.wrong_door = false;
                    x.reachable_state = ++s;
                }
            }
            return board;
        }

        public override void initSimpleNavigationFunction() {
            if (f == null)
                f = new OnWrongDoNothing();
        }
    }
}
