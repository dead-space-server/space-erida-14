using System.Runtime.CompilerServices;
using Content.Server._Erida.LightIntension;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._Erida.Nightmare.Components;
using Content.Shared.Actions;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Polymorph;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.Nightmare;

public sealed class NightmareSystem : SharedNightmareSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly LightIntensionSystem _lightIntension = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightmareComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<NightmareComponent, PolymorphActionEvent>(OnShadowWalkActionEvent, before:[typeof(PolymorphSystem)]);
    }

    private void OnInit(EntityUid uid, NightmareComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.ShadowWalkActionEntity, component.ShadowWalkAction);
    }

    public void OnShadowWalkActionEvent(Entity<NightmareComponent> ent, ref PolymorphActionEvent args)
    {
        if (TryComp<TransformComponent>(ent, out var xform)
            && TryComp<NightmareComponent>(ent, out var npComp)
            && !CheckCanTransformToPolymorph(ent, npComp, xform))
        {
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("Nightmare-failed-to-ShadowWalk"), ent, ent);
        }
    }

    private bool CheckCanTransformToPolymorph(EntityUid uid, NightmareComponent npComp, TransformComponent xform)
    {
        var gridUid = Transform(uid).GridUid;
        var inSpace = false;

        if (gridUid == null
            || !TryComp<MapGridComponent>(gridUid, out var grid)
            || _turf.IsSpace(_mapSystem.GetTileRef(gridUid.Value, grid, Transform(uid).Coordinates)))
        {
            inSpace = true;
        }

        if (inSpace)
        {
            return false;
        }

        var lightIntension = _lightIntension.TryGetLightLevel((uid, xform));

        if (lightIntension > npComp.redLineOfLight)
        {
            return false;
        }

        return true;
    }


    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<NightmareComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var nmComp, out var xform))
        {
            if (HasComp<PolymorphedEntityComponent>(uid))
                continue;

            if (nmComp.timeToCheck < curTime)
            {
                nmComp.timeToCheck = curTime + TimeSpan.FromSeconds(nmComp.timeBetweenChecks);

                var lightIntension = _lightIntension.TryGetLightLevel((uid, xform));
                if (lightIntension > nmComp.redLineOfLight)
                {
                    var scale = lightIntension - nmComp.redLineOfLight;
                    _damageable.TryChangeDamage(uid, nmComp.damageFromBurn * scale, true, false);
                    _audio.PlayPvs(nmComp.BurnSound, uid);
                }
                else
                {
                    _damageable.TryChangeDamage(uid, nmComp.healthFromDarkness, true, false);
                }
            }

            if (_mobState.IsDead(uid)
                && TryComp<MobThresholdsComponent>(uid, out var targetThresholds)
                && TryComp<DamageableComponent>(uid, out var targetDamageable)
                && _mobThreshold.TryGetThresholdForState(uid, MobState.Dead, out var threshold, targetThresholds)
                && _damageable.GetTotalDamage(uid) < threshold)
            {
                _mobState.ChangeMobState(uid, MobState.Critical);
            }
        }

        var query2 = EntityQueryEnumerator<NightmareComponent, PolymorphedEntityComponent, TransformComponent>();
        while (query2.MoveNext(out var uid, out var npComp, out var peComp, out var xform))
        {
            if (npComp.timeToCheck < curTime)
            {
                npComp.timeToCheck = curTime + TimeSpan.FromSeconds(npComp.timeBetweenChecksForShadowWalk);

                if (!CheckCanTransformToPolymorph(uid, npComp, xform))
                    _polymorphSystem.Revert((uid, peComp));
            }
        }
    }
}

public sealed partial class ShadowWalkActionEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<PolymorphPrototype>? ProtoId;

    public ShadowWalkActionEvent(ProtoId<PolymorphPrototype> protoId) : this()
    {
        ProtoId = protoId;
    }
    public sealed partial class RevertPolymorphActionEvent : InstantActionEvent;
}
