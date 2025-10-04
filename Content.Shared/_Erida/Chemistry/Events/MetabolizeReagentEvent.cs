// File: MetabolizeReagentEvent.cs
using Content.Shared.Chemistry.Reagent;

namespace Content.Shared.Chemistry.Events
{
    public sealed class MetabolizeReagentEvent : EventArgs
    {
        public EntityUid Target { get; } // Цель, на которую воздействует реагент
        public ReagentPrototype Reagent { get; } // Прототип реагента
        public float Amount { get; set; } // Количество реагента, подлежащее метаболизму

        public MetabolizeReagentEvent(EntityUid target, ReagentPrototype reagent, float amount)
        {
            Target = target;
            Reagent = reagent;
            Amount = amount;
        }
    }
}