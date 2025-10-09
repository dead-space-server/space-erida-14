// Shared/Chemistry/ReagentEffect.cs
using Robust.Shared.GameObjects;

namespace Content.Shared.Chemistry.Reagent;

public abstract class ReagentEffect
{
    // Общая точка входа для всех эффектов препаратов
    public virtual void ApplyEffect(EntityUid playerEnt, float toleranceLevel)
    {
        // Каждая конкретная реализация препарата должна переопределить этот метод
    }
}

public interface IChemicalEffectSystem : IEntitySystem
{
    // Контракты для регистрации системы на стороне сервера
}
