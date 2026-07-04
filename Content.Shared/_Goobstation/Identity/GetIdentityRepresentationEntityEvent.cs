namespace Content.Shared._Goobstation.Identity;

[ByRefEvent]
public record struct GetIdentityRepresentationEntityEvent(EntityUid? Uid = null);
