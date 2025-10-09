// Shared/Chemistry/Events/ChemicalReactionEvent.cs
using Robust.Shared.GameObjects;

namespace Content.Shared._Erida.Chemistry.Events;

public abstract class ChemicalReactionEvent : EventArgs
{
    public EntityUid User { get; }
    public string ReagentId { get; }
    public Type EffectType { get; }

    public ChemicalReactionEvent(EntityUid user, string reagentId, Type effectType)
    {
        User = user;
        ReagentId = reagentId;
        EffectType = effectType;
    }
}
