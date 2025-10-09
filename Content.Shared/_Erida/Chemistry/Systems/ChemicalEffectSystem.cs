// Shared/Chemistry/Systems/ChemicalEffectSystem.cs
using Robust.Shared.GameObjects;
using Content.Shared._Erida.Chemistry.Events;

namespace Content.Shared.Chemistry.Systems;

public abstract class ChemicalEffectSystem : EntitySystem
{
    // Обязательно абстрактный метод с точной сигнатурой
    public abstract void HandleChemicalReaction(ChemicalReactionEvent ev);
}
