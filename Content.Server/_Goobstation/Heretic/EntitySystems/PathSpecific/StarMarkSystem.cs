using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;

namespace Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;

public sealed partial class StarMarkSystem : SharedStarMarkSystem
{
    [Dependency] private AirtightSystem _airtight = default!;

    protected override void InitializeCosmicField(Entity<CosmicFieldComponent> field, int strength)
    {
        base.InitializeCosmicField(field, strength);

        if (strength < 7) // Cosmic blade level
            return;

        var airtight = EnsureComp<AirtightComponent>(field);
        airtight.BlockExplosions = true;
        _airtight.UpdatePosition((field.Owner, airtight));
    }
}
