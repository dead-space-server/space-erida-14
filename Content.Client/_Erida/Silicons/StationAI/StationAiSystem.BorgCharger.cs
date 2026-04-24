using Content.Shared.Silicons.StationAi;
using Content.Shared.Storage.Components;
using Robust.Shared.Utility;

namespace Content.Client.Silicons.StationAi;

public sealed partial class StationAiSystem
{
    private void InitializeBorgCharger()
    {
        SubscribeLocalEvent<EntityStorageComponent, GetStationAiRadialEvent>(OnBorgChargerGetRadial);
    }

    private void OnBorgChargerGetRadial(Entity<EntityStorageComponent> ent, ref GetStationAiRadialEvent args)
    {
        if (!HasComp<StationAiWhitelistComponent>(ent.Owner))
            return;

        args.Actions.Add(new StationAiRadial
        {
            Sprite = new SpriteSpecifier.Texture(new ResPath(ent.Comp.Open
                ? "/Textures/Interface/VerbIcons/close.svg.192dpi.png"
                : "/Textures/Interface/VerbIcons/open.svg.192dpi.png")),
            Tooltip = Loc.GetString(ent.Comp.Open
                ? "verb-common-close"
                : "verb-common-open"),
            Event = new StationAiToggleBorgChargerEvent()
        });
    }
}
