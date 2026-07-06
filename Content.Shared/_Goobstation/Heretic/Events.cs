using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.Heretic;

[ByRefEvent]
public readonly record struct ConsumingFoodEvent(EntityUid Food, FixedPoint2 Volume);

[ByRefEvent]
public record struct ImmuneToPoisonDamageEvent(bool Immune = false);
