namespace ConsoleApp2.probabilities
{
    public abstract class  BasicScenarios
    {
        internal ulong nStates { get; set;  }
        internal ulong nDoors {  get; set; }

        internal SimpleNavigationFunction f;

        public BasicScenarios(ulong nStates = 4, ulong nDoors = 6 )
        {
            this.nStates = nStates;
            this.nDoors = nDoors;
            this.f = null;
        }

        public abstract Board initScenario();
        public abstract void initSimpleNavigationFunction();

        public ulong runBestCaseScenario() {
            Board b = initScenario();
            initSimpleNavigationFunction();
            return SimpleNavigation.navigate(ref b, false, f);
        }

        public ulong runWorstCaseScenario() {
            Board b = initScenario();
            initSimpleNavigationFunction();
            return SimpleNavigation.navigate(ref b, true, f);
        }

    }
}
