// File: ReagentEffectModifier.cs
using Content.Server.Drugs.Components;
using Content.Shared.Chemistry.Events;

namespace Content.Server.Drugs.Systems
{
    public sealed class ReagentEffectModifier : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ApplyReagentEffectEvent>(HandleReagentEffect);
        }

        private void HandleReagentEffect(ApplyReagentEffectEvent ev)
        {
            var playerEnt = ev.Target;
            if (!EntityManager.TryGetComponent(playerEnt, out DrugToleranceComponent toleranceComp))
                return;

            // Уровень толерантности влияет на метаболизм вещества
            string? reagentID = ev.Reagent.ID;

            if (toleranceComp.Tolerances.TryGetValue(reagentID, out var tolerance))
            {
                // Ослабляем воздействие вещества
                ev.Amount *= 1 - tolerance / 100;
            }

            // Повышаем уровень толерантности
            toleranceComp.AddTolerance(reagentID, ev.Amount * 0.1f); // 10% от принятой дозы повышает толерантность
        }
    }
}
