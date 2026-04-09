using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.EntityConditions;
using Content.Shared._Lavaland.EntityEffects.EffectConditions;

namespace Content.Server._Lavaland.EntityEffects.Conditions;

/// <inheritdoc cref="EntityConditionSystem{T, TCon}"/>
public sealed partial class PressureThresholdEntityConditionSystem : EntityConditionSystem<TransformComponent, PressureThreshold>
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<PressureThreshold> args)
    {
        if (args.Condition.WorksOnLavaland && HasComp<LavalandMapComponent>(entity.Comp.MapUid))
        {
            args.Result = true;
            return;
        }

        var mix = _atmosphere.GetTileMixture(entity.AsNullable());
        var pressure = mix?.Pressure ?? 0f;
        args.Result = pressure >= args.Condition.Min && pressure <= args.Condition.Max;
    }
}
