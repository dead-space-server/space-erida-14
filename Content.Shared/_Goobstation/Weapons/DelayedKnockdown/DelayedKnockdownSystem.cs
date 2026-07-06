// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Components.PathSpecific;
using Content.Shared._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Shared.Armor;
using Content.Shared.Inventory;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;

namespace Content.Shared._Goobstation.Weapons.DelayedKnockdown;

public sealed class DelayedKnockdownSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ChampionStanceSystem _champion = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModifyDelayedKnockdownComponent, DelayedKnockdownAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ModifyDelayedKnockdownComponent, InventoryRelayedEvent<DelayedKnockdownAttemptEvent>>(
            OnInventoryAttempt);
        SubscribeLocalEvent<ModifyDelayedKnockdownComponent, ArmorExamineEvent>(OnExamine);

        SubscribeLocalEvent<ChampionStanceComponent, DelayedKnockdownAttemptEvent>(OnChampionDelayedKnockdownAttempt);
        SubscribeLocalEvent<SilverMaelstromComponent, DelayedKnockdownAttemptEvent>(OnMaelstromDelayedKnockdownAttempt);
    }

    private void OnMaelstromDelayedKnockdownAttempt(Entity<SilverMaelstromComponent> ent,
        ref DelayedKnockdownAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnChampionDelayedKnockdownAttempt(Entity<ChampionStanceComponent> ent,
        ref DelayedKnockdownAttemptEvent args)
    {
        if (!_champion.Condition(ent))
            return;

        args.Cancel();
    }

    private void OnExamine(Entity<ModifyDelayedKnockdownComponent> ent, ref ArmorExamineEvent args)
    {
        var comp = ent.Comp;

        if (comp.Cancel)
        {
            args.Msg.PushNewline();
            args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-cancel-delayed-knockdown"));
            return;
        }

        if (comp.DelayDelta != 0f)
        {
            args.Msg.PushNewline();
            args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-modify-delayed-knockdown-delay",
                ("amount", MathF.Abs(comp.DelayDelta)),
                ("deltasign", MathF.Sign(comp.DelayDelta))));
        }

        if (comp.KnockdownTimeDelta != 0f)
        {
            args.Msg.PushNewline();
            args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-modify-delayed-knockdown-time",
                ("amount", MathF.Abs(comp.KnockdownTimeDelta)),
                ("deltasign", MathF.Sign(comp.KnockdownTimeDelta))));
        }
    }

    private void OnInventoryAttempt(Entity<ModifyDelayedKnockdownComponent> ent,
        ref InventoryRelayedEvent<DelayedKnockdownAttemptEvent> args)
    {
        OnAttempt(ent, ref args.Args);
    }

    private void OnAttempt(Entity<ModifyDelayedKnockdownComponent> ent, ref DelayedKnockdownAttemptEvent args)
    {
        var comp = ent.Comp;

        if (comp.Cancel)
        {
            args.Cancel();
            return;
        }

        args.DelayDelta += comp.DelayDelta;
        args.KnockdownTimeDelta += comp.KnockdownTimeDelta;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DelayedKnockdownComponent, StatusEffectsComponent>();
        while (query.MoveNext(out var uid, out var comp, out var status))
        {
            comp.Time -= frameTime;

            if (comp.Time > 0)
                continue;

            _stun.TryKnockdown(uid, TimeSpan.FromSeconds(comp.KnockdownTime), comp.Refresh);

            RemCompDeferred(uid, comp);
        }
    }
}
