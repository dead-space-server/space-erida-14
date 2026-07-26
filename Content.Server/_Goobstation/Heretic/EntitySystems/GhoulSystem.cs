// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 github-actions <github-actions@github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Systems;
using Content.Server.Antag;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Dragon;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Hands.Components;
using Content.Server.Hands.Systems;
using Content.Server.Mind.Commands;
using Content.Server.Storage.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared.Body;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Examine;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared._Goobstation.Heretic;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.AnimalHusbandry;
using Content.Shared.Nutrition.Components;
using Content.Shared.RatKing;
using Robust.Server.Audio;
using Content.Shared._Goobstation.Religion;
using Content.Server.GameTicking.Rules;
using Content.Server._Goobstation.Heretic.Abilities;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Roles;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Coordinates;
using Content.Shared.Gibbing;
using Content.Shared.Roles;
using Content.Shared.Species.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Polymorph;
using Content.Server.Polymorph.Systems;
using Content.Shared.Administration.Systems;
using Content.Shared.Roles.Components;
using Content.Shared.Temperature.Components;

namespace Content.Server._Goobstation.Heretic.EntitySystems;

public sealed partial class GhoulSystem : EntitySystem
{
    private static readonly ProtoId<HTNCompoundPrototype> Compound = "HereticSummonCompound";
    private static readonly EntProtoId<MindRoleComponent> GhoulRole = "MindRoleGhoul";

    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private RejuvenateSystem _rejuvenate = default!;
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private StorageSystem _storage = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private HTNSystem _htn = default!;
    [Dependency] private SharedRoleSystem _role = default!;
    [Dependency] private HereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhoulComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GhoulComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GhoulComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<GhoulComponent, MobStateChangedEvent>(OnMobStateChange);

        SubscribeLocalEvent<GhoulRoleComponent, GetBriefingEvent>(OnGetBriefing);

        SubscribeLocalEvent<GhoulWeaponComponent, ExaminedEvent>(OnWeaponExamine);

        SubscribeLocalEvent<HereticMinionComponent, AttackAttemptEvent>(OnTryAttack);
        SubscribeLocalEvent<HereticMinionComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
    }

    private void OnGetBriefing(Entity<GhoulRoleComponent> ent, ref GetBriefingEvent args)
    {
        var uid = args.Mind.Comp.OwnedEntity;

        if (!TryComp(uid, out HereticMinionComponent? minion))
            return;

        var start = Loc.GetString("heretic-ghoul-briefing-start-noname");
        var master = minion.BoundHeretic;

        if (Exists(master))
        {
            start = Loc.GetString("heretic-ghoul-briefing-start",
                ("ent", Identity.Entity(master.Value, EntityManager)));
        }

        args.Append(start);
        args.Append(Loc.GetString("heretic-ghoul-briefing-end"));
    }

    private void OnWeaponExamine(Entity<GhoulWeaponComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    public void SetBoundHeretic(Entity<HereticMinionComponent?> ent, EntityUid heretic, bool dirty = true)
    {
        if (!_heretic.TryGetHereticComponent(heretic, out var hereticComp, out var mind))
            return;

        hereticComp.Minions.Add(ent);
        Dirty(mind, hereticComp);

        if (!Resolve(ent, ref ent.Comp, false))
            ent.Comp = AddComp<HereticMinionComponent>(ent);

        ent.Comp.BoundHeretic = heretic;
        _npc.SetBlackboard(ent, NPCBlackboard.FollowTarget, heretic.ToCoordinates());

        if (dirty)
            Dirty(ent);
    }

    public void GhoulifyEntity(Entity<GhoulComponent> ent)
    {
        RemComp<RespiratorComponent>(ent);
        RemComp<BarotraumaComponent>(ent);
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        RemComp<ReproductiveComponent>(ent);
        RemComp<ReproductivePartnerComponent>(ent);
        RemComp<TemperatureComponent>(ent);
        RemComp<PacifiedComponent>(ent);
        RemComp<RatKingComponent>(ent);
        RemComp<DragonComponent>(ent);
        EnsureComp<CombatModeComponent>(ent);

        if (TryComp(ent.Owner, out HereticMinionComponent? minion) && minion.BoundHeretic is { } heretic)
            SetBoundHeretic((ent.Owner, minion), heretic, false);

        _faction.ClearFactions(ent.Owner);
        _faction.AddFaction(ent.Owner, HereticSystem.HereticFactionId);

        var hasMind = _mind.TryGetMind(ent, out var mindId, out var mind);
        if (hasMind)
        {
            _mind.UnVisit(mindId, mind);
            if (!_role.MindHasRole<GhoulRoleComponent>(mindId))
            {
                SendBriefing(ent.Owner);
                _role.MindAddRole(mindId, GhoulRole, mind);
            }
        }
        else
        {
            var htn = EnsureComp<HTNComponent>(ent);
            htn.RootTask = new HTNCompoundTask { Task = Compound };
            _htn.Replan(htn);
        }

        _rejuvenate.PerformRejuvenate(ent);
        if (TryComp<MobThresholdsComponent>(ent, out var th))
        {
            _threshold.SetMobStateThreshold(ent, ent.Comp.TotalHealth, MobState.Dead, th);
            _threshold.SetMobStateThreshold(ent, ent.Comp.TotalHealth * 0.99f, MobState.Critical, th);
        }

        _mind.MakeSentient(ent);

        if (!hasMind)
        {
            var ghostRole = EnsureComp<GhostRoleComponent>(ent);
            ghostRole.RoleName = Loc.GetString(ent.Comp.GhostRoleName);
            ghostRole.RoleDescription = Loc.GetString(ent.Comp.GhostRoleDesc);
            ghostRole.RoleRules = Loc.GetString(ent.Comp.GhostRoleRules);
            ghostRole.MindRoles = [GhoulRole];
        }

        if (!HasComp<GhostRoleMobSpawnerComponent>(ent) && !hasMind)
            EnsureComp<GhostTakeoverAvailableComponent>(ent);

        if (TryComp(ent, out FleshMimickedComponent? mimicked))
        {
            foreach (var mimic in mimicked.FleshMimics)
            {
                if (!Exists(mimic))
                    continue;

                _faction.DeAggroEntity(mimic, ent);
            }

            RemCompDeferred(ent, mimicked);
        }

        if (!ent.Comp.GiveBlade || !TryComp(ent, out HandsComponent? hands))
            return;

        var blade = Spawn(ent.Comp.BladeProto, Transform(ent).Coordinates);
        EnsureComp<GhoulWeaponComponent>(blade);
        ent.Comp.BoundWeapon = blade;

        if (!_hands.TryPickup(ent, blade, animate: false, handsComp: hands) &&
            _inventory.TryGetSlotEntity(ent, "back", out var slotEnt) &&
            _storage.CanInsert(slotEnt.Value, blade, out _))
            _storage.Insert(slotEnt.Value, blade, out _, out _, playSound: false);
    }

    private void SendBriefing(Entity<HereticMinionComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var brief = Loc.GetString("heretic-ghoul-greeting-noname");
        var master = ent.Comp.BoundHeretic;

        if (Exists(master))
            brief = Loc.GetString("heretic-ghoul-greeting", ("ent", Identity.Entity(master.Value, EntityManager)));

        var sound = new SoundPathSpecifier("/Audio/_GoobStation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");
        _antag.SendBriefing(ent, brief, Color.MediumPurple, sound);
    }

    private void OnStartup(Entity<GhoulComponent> ent, ref ComponentStartup args)
    {
        GhoulifyEntity(ent);
        // var unholy = EnsureComp<WeakToHolyComponent>(ent);
        // unholy.AlwaysTakeHoly = true;
    }

    private void OnShutdown(Entity<GhoulComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.BoundWeapon == null || TerminatingOrDeleted(ent.Comp.BoundWeapon.Value))
            return;

        _audio.PlayPvs(ent.Comp.BladeDeleteSound, Transform(ent.Comp.BoundWeapon.Value).Coordinates);
        QueueDel(ent.Comp.BoundWeapon.Value);
    }

    private void OnTakeGhostRole(Entity<HereticMinionComponent> ent, ref TakeGhostRoleEvent args)
    {
        SendBriefing(ent.AsNullable());
    }

    private void OnTryAttack(Entity<HereticMinionComponent> ent, ref AttackAttemptEvent args)
    {
        if (args.Target == null)
            return;

        if (args.Target == ent.Comp.BoundHeretic || HasComp<ShadowCloakEntityComponent>(args.Target.Value) &&
            Transform(args.Target.Value).ParentUid == ent.Comp.BoundHeretic)
            args.Cancel();
    }

    private void OnExamine(Entity<GhoulComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineMessage == null)
            return;

        args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    private void OnMobStateChange(Entity<GhoulComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (ent.Comp.SpawnOnDeathPrototype != null)
            Spawn(ent.Comp.SpawnOnDeathPrototype.Value, Transform(ent).Coordinates);

        if (!TryComp(ent, out BodyComponent? body))
            return;

        if (_body.TryGetOrgansWithComponent<NymphComponent>((ent, body), out var nymphs))
        {
            foreach (var nymph in nymphs)
                RemComp(nymph.Owner, nymph.Comp);
        }

        _gibbing.Gib(ent, ent.Comp.DropOrgansOnDeath);
    }
}
