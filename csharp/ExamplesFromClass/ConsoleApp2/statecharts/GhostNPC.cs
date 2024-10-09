// Autogenerated with StateSmith 0.17.0+31be1fc54adc06a86e0e6ed18288dcac427197fe.
// Algorithm: Balanced2. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

// Whatever you put in this `FileTop` section will end up 
// being printed at the top of every generated code file.


// Generated state machine
public partial class GhostNPC
{
    public enum EventId
    {
        GHOSTEATEN = 0,
        NOPLAYER = 1,
        PLAYERPOWERUP = 2,
        POWERUPEXPIRY = 3,
        REACHEDBASE = 4,
        SEEPLAYER = 5,
    }

    public const int EventIdCount = 6;

    public enum StateId
    {
        ROOT = 0,
        CHASEPLAYER = 1,
        MOVETOBASE = 2,
        RUNAWAY = 3,
        WANDERMAZE = 4,
    }

    public const int StateIdCount = 5;

    // Used internally by state machine. Feel free to inspect, but don't modify.
    public StateId stateId;

    // State machine variables. Can be used for inputs, outputs, user variables...
    public struct Vars
    {
        public MyTime timer; // this var can be referenced in diagram
        public long count;
    }

    // Variables. Can be used for inputs, outputs, user variables...
    public Vars vars = new Vars();

    // State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
    public GhostNPC()
    {
    }

    // Starts the state machine. Must be called before dispatching events. Not thread safe.
    public void Start()
    {
        ROOT_enter();
        // ROOT behavior
        // uml: TransitionTo(ROOT.<InitialState>)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `ROOT.<InitialState>`.
            // ROOT.<InitialState> is a pseudo state and cannot have an `enter` trigger.

            // ROOT.<InitialState> behavior
            // uml: TransitionTo(WanderMaze)
            {
                // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.

                // Step 2: Transition action: ``.

                // Step 3: Enter/move towards transition target `WanderMaze`.
                WANDERMAZE_enter();

                // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
                return;
            } // end of behavior for ROOT.<InitialState>
        } // end of behavior for ROOT
    }

    // Dispatches an event to the state machine. Not thread safe.
    // Note! This function assumes that the `eventId` parameter is valid.
    public void DispatchEvent(EventId eventId)
    {
        switch (this.stateId)
        {
            // STATE: GhostNPC
            case StateId.ROOT:
                // No events handled by this state (or its ancestors).
                break;

            // STATE: ChasePlayer
            case StateId.CHASEPLAYER:
                switch (eventId)
                {
                    case EventId.NOPLAYER: CHASEPLAYER_noplayer(); break;
                    case EventId.PLAYERPOWERUP: CHASEPLAYER_playerpowerup(); break;
                    case EventId.GHOSTEATEN: CHASEPLAYER_ghosteaten(); break;
                }
                break;

            // STATE: MoveToBase
            case StateId.MOVETOBASE:
                switch (eventId)
                {
                    case EventId.REACHEDBASE: MOVETOBASE_reachedbase(); break;
                }
                break;

            // STATE: RunAway
            case StateId.RUNAWAY:
                switch (eventId)
                {
                    case EventId.POWERUPEXPIRY: RUNAWAY_powerupexpiry(); break;
                }
                break;

            // STATE: WanderMaze
            case StateId.WANDERMAZE:
                switch (eventId)
                {
                    case EventId.SEEPLAYER: WANDERMAZE_seeplayer(); break;
                    case EventId.PLAYERPOWERUP: WANDERMAZE_playerpowerup(); break;
                }
                break;
        }

    }

    // This function is used when StateSmith doesn't know what the active leaf state is at
    // compile time due to sub states or when multiple states need to be exited.
    private void ExitUpToStateHandler(StateId desiredState)
    {
        while (this.stateId != desiredState)
        {
            switch (this.stateId)
            {
                case StateId.CHASEPLAYER: CHASEPLAYER_exit(); break;

                case StateId.MOVETOBASE: MOVETOBASE_exit(); break;

                case StateId.RUNAWAY: RUNAWAY_exit(); break;

                case StateId.WANDERMAZE: WANDERMAZE_exit(); break;

                default: return;  // Just to be safe. Prevents infinite loop if state ID memory is somehow corrupted.
            }
        }
    }


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ROOT
    ////////////////////////////////////////////////////////////////////////////////

    private void ROOT_enter()
    {
        this.stateId = StateId.ROOT;
    }


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state CHASEPLAYER
    ////////////////////////////////////////////////////////////////////////////////

    private void CHASEPLAYER_enter()
    {
        this.stateId = StateId.CHASEPLAYER;
    }

    private void CHASEPLAYER_exit()
    {
        this.stateId = StateId.ROOT;
    }

    private void CHASEPLAYER_ghosteaten()
    {
        // ChasePlayer behavior
        // uml: GhostEaten TransitionTo(MoveToBase)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            CHASEPLAYER_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `MoveToBase`.
            MOVETOBASE_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for ChasePlayer

        // No ancestor handles this event.
    }

    private void CHASEPLAYER_noplayer()
    {
        // ChasePlayer behavior
        // uml: NoPlayer TransitionTo(WanderMaze)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            CHASEPLAYER_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `WanderMaze`.
            WANDERMAZE_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for ChasePlayer

        // No ancestor handles this event.
    }

    private void CHASEPLAYER_playerpowerup()
    {
        // ChasePlayer behavior
        // uml: PlayerPowerUp TransitionTo(RunAway)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            CHASEPLAYER_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `RunAway`.
            RUNAWAY_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for ChasePlayer

        // No ancestor handles this event.
    }


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state MOVETOBASE
    ////////////////////////////////////////////////////////////////////////////////

    private void MOVETOBASE_enter()
    {
        this.stateId = StateId.MOVETOBASE;
    }

    private void MOVETOBASE_exit()
    {
        this.stateId = StateId.ROOT;
    }

    private void MOVETOBASE_reachedbase()
    {
        // MoveToBase behavior
        // uml: ReachedBase TransitionTo(WanderMaze)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            MOVETOBASE_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `WanderMaze`.
            WANDERMAZE_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for MoveToBase

        // No ancestor handles this event.
    }


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state RUNAWAY
    ////////////////////////////////////////////////////////////////////////////////

    private void RUNAWAY_enter()
    {
        this.stateId = StateId.RUNAWAY;
    }

    private void RUNAWAY_exit()
    {
        this.stateId = StateId.ROOT;
    }

    private void RUNAWAY_powerupexpiry()
    {
        // RunAway behavior
        // uml: PowerUpExpiry TransitionTo(WanderMaze)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            RUNAWAY_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `WanderMaze`.
            WANDERMAZE_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for RunAway

        // No ancestor handles this event.
    }


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state WANDERMAZE
    ////////////////////////////////////////////////////////////////////////////////

    private void WANDERMAZE_enter()
    {
        this.stateId = StateId.WANDERMAZE;
    }

    private void WANDERMAZE_exit()
    {
        this.stateId = StateId.ROOT;
    }

    private void WANDERMAZE_playerpowerup()
    {
        // WanderMaze behavior
        // uml: PlayerPowerUp TransitionTo(RunAway)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            WANDERMAZE_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `RunAway`.
            RUNAWAY_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for WanderMaze

        // No ancestor handles this event.
    }

    private void WANDERMAZE_seeplayer()
    {
        // WanderMaze behavior
        // uml: SeePlayer TransitionTo(ChasePlayer)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            WANDERMAZE_exit();

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `ChasePlayer`.
            CHASEPLAYER_enter();

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for WanderMaze

        // No ancestor handles this event.
    }

    // Thread safe.
    public static string StateIdToString(StateId id)
    {
        switch (id)
        {
            case StateId.ROOT: return "ROOT";
            case StateId.CHASEPLAYER: return "CHASEPLAYER";
            case StateId.MOVETOBASE: return "MOVETOBASE";
            case StateId.RUNAWAY: return "RUNAWAY";
            case StateId.WANDERMAZE: return "WANDERMAZE";
            default: return "?";
        }
    }

    // Thread safe.
    public static string EventIdToString(EventId id)
    {
        switch (id)
        {
            case EventId.GHOSTEATEN: return "GHOSTEATEN";
            case EventId.NOPLAYER: return "NOPLAYER";
            case EventId.PLAYERPOWERUP: return "PLAYERPOWERUP";
            case EventId.POWERUPEXPIRY: return "POWERUPEXPIRY";
            case EventId.REACHEDBASE: return "REACHEDBASE";
            case EventId.SEEPLAYER: return "SEEPLAYER";
            default: return "?";
        }
    }
}
