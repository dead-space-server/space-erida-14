// File: ReagentEffectModifier.cs
using Content.Shared.Chemistry.Events;
using Content.Shared.Drugs.Components;

namespace Content.Shared.Tolerance.Systems
{
    public sealed class ReagentEffectModifier : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MetabolizeReagentEvent>(HandleMetabolizeReagent);
        }

        private void HandleMetabolizeReagent(MetabolizeReagentEvent ev)
        {
            var playerEnt = ev.Target;
            if (!EntityManager.TryGetComponent(playerEnt, out DrugToleranceComponent toleranceComp))
                return;

            // Проверяем, что ID не равен null
            if (ev.Reagent.ID != null)
            {
                var reagentID = ev.Reagent.ID;

                if (toleranceComp.Tolerances.TryGetValue(reagentID, out var tolerance))
                {
                    // Ослабляем воздействие вещества
                    ev.Amount *= 1 - tolerance / 100;
                }
            }
            else
            {
                // Обработка ситуации, когда ID равен null
            }
        }
    }
}
