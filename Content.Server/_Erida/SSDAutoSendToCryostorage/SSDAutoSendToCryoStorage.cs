using Content.Server._Erida.SSDAutoSendToCryostorage.Components;
using Content.Server.Administration.Logs;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.SSDIndicator;
using Robust.Server.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server._Erida.SSDAutoSendToCryostorage;

public sealed class SSDAutoSendToCryostorageSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private bool _icSsdSendToCryostorage;
    private float _icSsdSendToCryostorageTime;
    public override void Initialize()
    {
        SubscribeLocalEvent<SSDAutoSendToCryostorageComponent, PlayerAttachedEvent>(OnPlayerAttached, after: [typeof(SSDIndicatorSystem)]);
        SubscribeLocalEvent<SSDAutoSendToCryostorageComponent, PlayerDetachedEvent>(OnPlayerDetached, after: [typeof(SSDIndicatorSystem)]);

        SubscribeLocalEvent<SSDAutoSendToCryostorageComponent, ExaminedEvent>(OnExamined);

        _cfg.OnValueChanged(CCVars.ICSSDAutoSendToCryostorage, obj => _icSsdSendToCryostorage = obj, true);
        _cfg.OnValueChanged(CCVars.ICSSDAutoSendToCryostorageTime, obj => _icSsdSendToCryostorageTime = obj, true);
    }

    private void OnPlayerAttached(Entity<SSDAutoSendToCryostorageComponent> ent, ref PlayerAttachedEvent args)
    {
        if (!_icSsdSendToCryostorage
            || !TryComp<MindContainerComponent>(ent, out var mindContainerComp)
            || !mindContainerComp.HasMind)
            return;

        ent.Comp.Active = false;
        ent.Comp.SendToCryostorageTime = TimeSpan.Zero;
    }

    private void OnPlayerDetached(Entity<SSDAutoSendToCryostorageComponent> ent, ref PlayerDetachedEvent args)
    {
        if (!_icSsdSendToCryostorage
            || !TryComp<MindContainerComponent>(ent, out var mindContainerComp)
            || !mindContainerComp.HasMind
            || !TryComp<MobStateComponent>(ent, out var stateComp)
            || _mobState.IsDead(ent, stateComp))
        {
            return;
        }

        ent.Comp.Active = true;
        ent.Comp.SendToCryostorageTime = _timing.CurTime + TimeSpan.FromSeconds(_icSsdSendToCryostorageTime);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_icSsdSendToCryostorage)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SSDAutoSendToCryostorageComponent, TransformComponent, MetaDataComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var ssd, out var xfrom, out var meta, out var physics))
        {
            if (ssd.SendToCryostorageTime > curTime
                || !ssd.Active)
                continue;

            if (!SendToCryostorage(new Entity<TransformComponent?, MetaDataComponent?, PhysicsComponent?>(uid, xfrom, meta, physics), ref ssd))
            {
                // Try next time
                _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid):uid} not found cryo after to send for a long absence");
                ssd.SendToCryostorageTime = curTime + TimeSpan.FromSeconds(_icSsdSendToCryostorageTime);
                continue;
            }

            ssd.Active = false;
        }
    }

    private bool SendToCryostorage(Entity<TransformComponent?, MetaDataComponent?, PhysicsComponent?> uid, ref SSDAutoSendToCryostorageComponent sSDcomp)
    {
        var (xform, meta, physics) = (uid.Comp1, uid.Comp2, uid.Comp3);

        if (!Resolve(uid, ref xform, ref meta, ref physics))
            return false;

        var playerPos = xform.Coordinates;
        EntityUid? bestCryo = null;
        float bestDistance = float.PositiveInfinity;
        BaseContainer? bestContainer = null;

        var query = EntityQueryEnumerator<CryostorageComponent>();
        while (query.MoveNext(out var cryoUid, out var cryoComp))
        {
            if (!_container.TryGetContainer(cryoUid, cryoComp.ContainerId, out var container)
                || container.Count != 0
                || !_entityManager.TryGetComponent<TransformComponent>(cryoUid, out var cryoXform)
                || !playerPos.TryDistance(_entityManager, cryoXform.Coordinates, out var distance))
                continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCryo = cryoUid;
                bestContainer = container;
            }
        }

        if (bestCryo == null
            || bestContainer == null)
        {
            return false;
        }

        _audio.PlayPvs(sSDcomp.SoundSend, uid);
        var entEffect = SpawnAtPosition(sSDcomp.EntityEffect, playerPos);
        if (!TryComp<TimedDespawnComponent>(entEffect, out var _))
        {
            var tDComp = AddComp<TimedDespawnComponent>(entEffect);
            tDComp.Lifetime = 0.5f;
        }
        _container.Insert(uid, bestContainer);
        _audio.PlayPvs(sSDcomp.SoundExit, uid);

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid.Owner):entity} sent to cryo after a long absence");

        return true;
    }

    private void OnExamined(Entity<SSDAutoSendToCryostorageComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || !ent.Comp.Active)
            return;

        var timeToSend = Math.Round((ent.Comp.SendToCryostorageTime - _timing.CurTime).TotalSeconds).ToString();

        var message = $"[color=yellow]{Loc.GetString("comp-SSDAutoSendToCryostorage-examined-active", ("time", timeToSend))}[/color]";

        args.PushMarkup(message);
    }
}
