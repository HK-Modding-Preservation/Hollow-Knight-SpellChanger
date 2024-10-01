namespace SpellChanger.Utils
{
    internal static class FsmUtil
    {
        public static void AddTransition(this FsmState state, FsmEvent @event, FsmState to)
        {
            state.Transitions = state.Transitions.Append(new FsmTransition
            {
                FsmEvent = @event,
                ToFsmState = to
            }).ToArray();
        }

        public static FsmEvent CreateFsmEvent(this PlayMakerFSM fsm, string eventName)
        {
            var @new = new FsmEvent(eventName);

            fsm.Fsm.Events = fsm.Fsm.Events.Append(@new).ToArray();

            return @new;
        }

        public static FsmEvent CreateFsmEvent(this PlayMakerFSM fsm, FsmEvent fsmEvent)
        {

            fsm.Fsm.Events = fsm.Fsm.Events.Append(fsmEvent).ToArray();

            return fsmEvent;
        }

        public static FsmEvent GetFsmEvent(this PlayMakerFSM fsm, string eventName)
        {
            foreach (FsmEvent Event in fsm.Fsm.Events)
            {
                if (Event.Name == eventName) { return Event; }

            }

            return null;
        }
    }
}
