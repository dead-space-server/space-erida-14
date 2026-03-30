using Content.Shared._Erida.SSDAutoSendToCryostage.Components;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.CCVar;
using Content.Shared.Medical.Cryogenics;
using Content.Shared.Mind;
using Content.Shared.SSDIndicator;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Shared._Erida.SSDAutoSendToCryostage;

public sealed class SSDAutoSendToCryostorageSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    private bool _icSsdSendToCryostorage;
    private float _icSsdSendToCryostorageTime;
    public override void Initialize()
    {
        SubscribeLocalEvent<SSDAutoSendToCryostageComponent, PlayerAttachedEvent>(OnPlayerAttached, after: [typeof(SSDIndicatorSystem)]);
        SubscribeLocalEvent<SSDAutoSendToCryostageComponent, PlayerDetachedEvent>(OnPlayerDetached, after: [typeof(SSDIndicatorSystem)]);

        _cfg.OnValueChanged(CCVars.ICSSDAutoSendToCryostorage, obj => _icSsdSendToCryostorage = obj, true);
        _cfg.OnValueChanged(CCVars.ICSSDAutoSendToCryostorageTime, obj => _icSsdSendToCryostorageTime = obj, true);
    }

    private void OnPlayerAttached(Entity<SSDAutoSendToCryostageComponent> ent, ref PlayerAttachedEvent args)
    {
        Log.Debug("OnPlayerAttached Рил работает");
        if (!_icSsdSendToCryostorage
            || !TryComp<SSDIndicatorComponent>(ent, out var sSDIndComp)
            || !TryComp<MindComponent>(ent, out var mind)
            || !mind.UserId.HasValue)
            return;

        Log.Debug("OnPlayerAttached и идёт дальше");
        ent.Comp.IsSSD = sSDIndComp.IsSSD;
        ent.Comp.SendToCryostorageTime = TimeSpan.Zero;
    }

    private void OnPlayerDetached(Entity<SSDAutoSendToCryostageComponent> ent, ref PlayerDetachedEvent args)
    {
        Log.Debug("OnPlayerDetached Рил работает");
        Log.Debug($"Проверки: {!_icSsdSendToCryostorage} {!TryComp<SSDIndicatorComponent>(ent, out var sSDIndComp1)} {!HasComp<MindComponent>(ent)}");
        if (!_icSsdSendToCryostorage
            || !TryComp<SSDIndicatorComponent>(ent, out var sSDIndComp)
            || !TryComp<MindComponent>(ent, out var mind)
            || !mind.UserId.HasValue)
        {
            return;
        }


        Log.Debug("OnPlayerDetached и идёт дальше");
        ent.Comp.IsSSD = sSDIndComp.IsSSD;
        ent.Comp.SendToCryostorageTime = _timing.CurTime + TimeSpan.FromSeconds(_icSsdSendToCryostorageTime);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_icSsdSendToCryostorage)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SSDAutoSendToCryostageComponent, TransformComponent, MetaDataComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var ssd, out var xfrom, out var meta, out var physics))
        {
            if (ssd.SendToCryostorageTime > curTime)
                continue;

            SendToCryostorage(new Entity<TransformComponent?, MetaDataComponent?, PhysicsComponent?>(uid, xfrom, meta, physics));
        }
    }

    private bool SendToCryostorage(Entity<TransformComponent?, MetaDataComponent?, PhysicsComponent?> uid)
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
                || !TryComp<TransformComponent>(cryoUid, out var cryoXform)
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
            Log.Warning($"Не найдено свободной криокамеры для {ToPrettyString(uid)}");
            return false;
        }

        if (_container.Insert(uid, bestContainer))
        {
            Log.Info($"{ToPrettyString(uid)} автоматически отправлен в криокамеру {ToPrettyString(bestCryo.Value)}");
        }

        return true;
    }
}
