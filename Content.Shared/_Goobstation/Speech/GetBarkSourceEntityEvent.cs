namespace Content.Shared._Goobstation.Speech;

[ByRefEvent]
public record struct GetBarkSourceEntityEvent(EntityUid? Ent = null);
