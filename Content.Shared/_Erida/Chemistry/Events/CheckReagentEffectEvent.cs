using Content.Shared.Chemistry.Reagent;

namespace Content.Shared.Chemistry.Events
{
    public sealed class CheckReagentEffectEvent : EventArgs
    {
        public EntityUid Target { get; init; }
        public ReagentPrototype Reagent { get; init; }
        public float Amount { get; set; }

        public CheckReagentEffectEvent(EntityUid target, ReagentPrototype reagent, float amount)
        {
            Target = target;
            Reagent = reagent;
            Amount = amount;
        }
    }
}
