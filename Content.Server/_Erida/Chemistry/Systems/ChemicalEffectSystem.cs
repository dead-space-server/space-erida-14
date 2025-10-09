// Server/Chemistry/Systems/ChemicalEffectSystem.cs
using Content.Server.Player.Components;
using Content.Server.Events.Chemistry;
using Content.Shared.Chemistry.Reagent;

namespace Content.Server.Chemistry.Systems;

public sealed class ChemicalEffectSystem
{
    private readonly IEntityManager _entityManager;

    public ChemicalEffectSystem(IEntityManager entityManager)
    {
        _entityManager = entityManager;

    }

    // Переопределяем абстрактный метод
    public void HandleChemicalReaction(ChemicalReactionEvent ev)
    {

        // Ваша логика обработки химической реакции
        if (_entityManager.TryGetComponent(ev.User, out DrugToleranceComponent? toleranceComp))
        {
            if (toleranceComp.Tolerances.ContainsKey(ev.ReagentId))
            {
                // Уменьшаем эффект в зависимости от текущего уровня толерантности
                float currentTolerance = toleranceComp.Tolerances[ev.ReagentId];
                ApplyReducedEffect(ev.User, CreateReagentEffect(ev.EffectType), currentTolerance);

                // Повышаем уровень толерантности
                IncreaseTolerance(toleranceComp, ev.ReagentId);
            }
            else
            {
                // Препарат применяется впервые, устанавливаем начальную толерантность
                toleranceComp.IncreaseTolerance(ev.ReagentId, 0.1f);
                ApplyFullEffect(ev.User, CreateReagentEffect(ev.EffectType));
            }
        }
        else
        {
            // Без компонента толерантности применяем полный эффект
            ApplyFullEffect(ev.User, CreateReagentEffect(ev.EffectType));
        }
    }

    // Остальные методы...

    private ReagentEffect CreateReagentEffect(Type type)
    {
        return Activator.CreateInstance(type) as ReagentEffect ??
               throw new InvalidOperationException($"Невозможно создать экземпляр типа {type.FullName}");
    }

    private void ApplyReducedEffect(EntityUid playerEnt, ReagentEffect effectType, float toleranceLevel)
    {
        effectType.ApplyEffect(playerEnt, toleranceLevel);
    }

    private void ApplyFullEffect(EntityUid playerEnt, ReagentEffect effectType)
    {
        effectType.ApplyEffect(playerEnt, 0f); // Значение 0f обозначает отсутствие толерантности
    }

    private void IncreaseTolerance(DrugToleranceComponent comp, string reagentId)
    {
        if (comp.Tolerances.TryGetValue(reagentId, out float currentTol))
        {
            comp.Tolerances[reagentId] += 0.9f; // Повышаем уровень толерантности на 10%
        }
        else
        {
            comp.Tolerances.Add(reagentId, 0.9f); // Устанавливаем минимальный уровень толерантности
        }
    }
}
