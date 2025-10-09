// Events/Chemistry/ChemicalReactionEvent.cs
using Robust.Shared.Serialization;

namespace Content.Server.Events.Chemistry;

[Serializable]
public sealed class ChemicalReactionEvent : HandledEntityEventArgs
{
    // Идентификатор принимаемого вещества
    public string ReagentId { get; init; }

    // Тип воздействия вещества
    public Type EffectType { get; init; }

    // Пользователь (игрок), применивший химическое вещество
    public EntityUid User { get; init; }

    // Объект взаимодействия (например, контейнер, куда было введено вещество)
    public EntityUid Target { get; init; }

    // Конструктор класса
    public ChemicalReactionEvent(string reagentId, Type effectType, EntityUid user, EntityUid target)
    {
        ReagentId = reagentId;
        EffectType = effectType;
        User = user;
        Target = target;
    }
}
