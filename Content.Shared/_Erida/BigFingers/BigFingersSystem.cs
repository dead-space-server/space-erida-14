using Content.Shared._Erida.BigFingers.Components;
using Content.Shared.Clothing;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Erida.BigFingers;

public sealed partial class BigFingersSystem : EntitySystem
{
    [Dependency] protected SharedPopupSystem Popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BigFingersComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<BigFingersComponent, ClothingGotEquippedEvent>(OnClothingGotEquipped);
        SubscribeLocalEvent<BigFingersComponent, ClothingGotUnequippedEvent>(OnClothingGotUnequipped);
    }

    private void OnShotAttempted(Entity<BigFingersComponent> uid, ref ShotAttemptedEvent args)
    {
        if (_timing.CurTime > uid.Comp.NextPopupTime
            || uid.Comp.NextPopupTime == null)
        {
            Popup.PopupClient(Loc.GetString("too-big-fingers"), uid, uid);
            uid.Comp.NextPopupTime = _timing.CurTime + uid.Comp.PopupCooldown;
        }

        args.Cancel();
    }

    private void OnClothingGotEquipped(Entity<BigFingersComponent> uid, ref ClothingGotEquippedEvent args)
    {
        if (HasComp<BigFingersComponent>(args.Wearer))
            return;

        var bfComp = AddComp<BigFingersComponent>(args.Wearer);
        bfComp.ByClothes = true;
    }

    private void OnClothingGotUnequipped(Entity<BigFingersComponent> uid, ref ClothingGotUnequippedEvent args)
    {
        if (TryComp<BigFingersComponent>(args.Wearer, out var bfComp)
            && !bfComp.ByClothes)
            return;

        RemComp<BigFingersComponent>(args.Wearer);
    }
}
