// File: ApplyReagentEffectEvent.cs
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameObjects;

namespace Content.Shared.Chemistry.Events
{
    public sealed class ApplyReagentEffectEvent : EventArgs
    {
        public EntityUid Target { get; } // Целевое существо или объект
        public ReagentPrototype Reagent { get; } // Прототип реагента
        public float Amount { get; set; } // Кол-во реагента, которое применяется

        public ApplyReagentEffectEvent(EntityUid target, ReagentPrototype reagent, float amount)
        {
            Target = target;
            Reagent = reagent;
            Amount = amount;
        }
    }
}
