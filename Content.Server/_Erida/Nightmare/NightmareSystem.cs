using System.Linq;
using System.Runtime.CompilerServices;
using Content.Server._Erida.LightIntension;
using Content.Server._Erida.Nightmare.Components;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared._Erida.Nightmare.Components;
using Content.Shared._Erida.Roles.Components;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Maps;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Polymorph;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
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
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly FixtureSystem _fixture = default!;
    [Dependency] private readonly IEntityManager _manager = default!;
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightmareComponent, MapInitEvent>(OnInit, after: [typeof(PolymorphSystem)]);
        SubscribeLocalEvent<NightmareComponent, PolymorphActionEvent>(OnShadowWalkActionEvent, before: [typeof(PolymorphSystem)]);
        SubscribeLocalEvent<NightmareComponent, RevertPolymorphActionEvent>(OnRevertShadowWalkActionEvent, before: [typeof(PolymorphSystem)]);
        SubscribeLocalEvent<NightmareComponent, PolymorphedEvent>(OnPolymorphedEvent, before: [typeof(PolymorphSystem)]);

        SubscribeLocalEvent<NightmareRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<NightmareComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<NightmareComponent, MindRemovedMessage>(OnMindRemoved);
    }

    private void OnInit(EntityUid uid, NightmareComponent component, MapInitEvent args)
    {
        if (!HasComp<NightmarePolymorhedComponent>(uid))
        {
            _action.AddAction(uid, ref component.ShadowWalkActionEntity, component.ShadowWalkAction);

            UpdateOldFixture(uid, component);

            var xform = _manager.GetComponent<TransformComponent>(uid);

            if (xform == null)
                return;

            var lightIntension = _lightIntension.TryGetLightLevel((uid, xform));
            if (lightIntension <= component.RedLineOfLight)
            {
                ChangeLayerMask(uid, component);
            }
        }
    }

    public void OnShadowWalkActionEvent(Entity<NightmareComponent> ent, ref PolymorphActionEvent args)
    {
        if (_entityManager.TryGetComponent<TransformComponent>(ent, out var xform)
            && TryComp<NightmareComponent>(ent, out var npComp)
            && !CheckCanTransformToPolymorph(ent, npComp, xform))
        {
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("nightmare-failed-to-shadowwalk"), ent, ent);
        }

        if (_mindSystem.TryGetMind(ent, out var mindId, out var _)
        && _role.MindHasRole<NightmareRoleComponent>(mindId, out var nRole))
            nRole.Value.Comp2.PolymorphState = true;
    }

    public void OnRevertShadowWalkActionEvent(Entity<NightmareComponent> ent, ref RevertPolymorphActionEvent args)
    {
        if (_mindSystem.TryGetMind(ent, out var mindId, out var _)
        && _role.MindHasRole<NightmareRoleComponent>(mindId, out var nRole))
            nRole.Value.Comp2.PolymorphState = true;
    }

    public void OnPolymorphedEvent(Entity<NightmareComponent> ent, ref PolymorphedEvent args)
    {
        if (_mindSystem.TryGetMind(ent, out var mindId, out var _)
        && _role.MindHasRole<NightmareRoleComponent>(mindId, out var nRole))
            nRole.Value.Comp2.PolymorphState = false;
    }

    private bool CheckCanTransformToPolymorph(EntityUid uid, NightmareComponent npComp, TransformComponent xform)
    {
        var gridUid = Transform(uid).GridUid;

        if (gridUid == null
            || !TryComp<MapGridComponent>(gridUid, out var grid)
            || _turf.IsSpace(_mapSystem.GetTileRef(gridUid.Value, grid, Transform(uid).Coordinates)))
        {
            return false;
        }

        var lightIntension = _lightIntension.TryGetLightLevel((uid, xform));

        if (lightIntension > npComp.RedLineOfLight)
            return false;

        return true;
    }

    private void OnGetBriefing(EntityUid uid, NightmareRoleComponent comp, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("nightmare-briefing"));
    }

    private void OnMindAdded(EntityUid uid, NightmareComponent component, MindAddedMessage args)
    {
        if (!_role.MindHasRole<NightmareRoleComponent>(args.Mind.Owner))
        {
            Start(uid, component);
        }
    }

    private void OnMindRemoved(EntityUid uid, NightmareComponent component, MindRemovedMessage args)
    {
        if (_role.MindHasRole<NightmareRoleComponent>(args.Mind.Owner, out var nRole)
        && nRole.Value.Comp2.PolymorphState)
            return;

        _role.MindRemoveRole<NightmareRoleComponent>(args.Mind.Owner);
    }

    public void Start(EntityUid uid, NightmareComponent comp)
    {
        if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind))
        {
            return;
        }

        _role.MindAddRole(mindId, "MindRoleNightmare");

        if (mind is { UserId: not null } && _player.TryGetSessionById(mind.UserId, out var session))
            _antag.SendBriefing(session, Loc.GetString("nightmare-role-greeting"), null, null);
    }

    private void ChangeLayerMask(EntityUid uid, NightmareComponent nmComp)
    {
        if (!TryComp<FixturesComponent>(uid, out var fComp))
            return;

        if (nmComp.OldLayerMask == null)
            return;

        var currFixture = _fixture.GetFixtureOrNull(uid, "fix1", fComp);

        if (currFixture == null)
            return;

        _physicsSystem.SetCollisionLayer(uid, "fix1", currFixture, nmComp.NewLayerMask);
    }

    private void ReturnOldFixture(EntityUid uid, NightmareComponent nmComp)
    {
        if (!TryComp<FixturesComponent>(uid, out var fComp))
            return;

        if (nmComp.OldLayerMask == null)
            return;

        var currFixture = _fixture.GetFixtureOrNull(uid, "fix1", fComp);

        if (currFixture == null)
            return;

        _physicsSystem.SetCollisionLayer(uid, "fix1", currFixture, nmComp.OldLayerMask.Value);
    }

    private void UpdateOldFixture(EntityUid uid, NightmareComponent nmComp)
    {
        if (!TryComp<FixturesComponent>(uid, out var fComp))
            return;

        var currFixture = _fixture.GetFixtureOrNull(uid, "fix1", fComp);

        if (currFixture != null)
            nmComp.OldLayerMask = currFixture.CollisionLayer;
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<NightmareComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var nmComp, out var xform))
        {
            if (HasComp<PolymorphedEntityComponent>(uid))
                continue;

            if (nmComp.TimeToCheck < curTime)
            {
                nmComp.TimeToCheck = curTime + TimeSpan.FromSeconds(nmComp.TimeBetweenChecks);

                var lightIntension = _lightIntension.TryGetLightLevel((uid, xform));
                //Logger.Debug($"lightIntension: {lightIntension}");
                if (lightIntension > nmComp.RedLineOfLight)
                {
                    if (nmComp.InTheDark)
                    {
                        nmComp.InTheDark = false;

                        ReturnOldFixture(uid, nmComp);
                    }

                    var scale = lightIntension - nmComp.RedLineOfLight;
                    _damageable.TryChangeDamage(uid, nmComp.DamageFromBurn * scale, true, false);
                    _audio.PlayPvs(nmComp.BurnSound, uid);
                }
                else
                {
                    _damageable.TryChangeDamage(uid, nmComp.HealthFromDarkness, true, false);

                    if (!nmComp.InTheDark)
                    {
                        nmComp.InTheDark = true;

                        //UpdateOldFixture(uid, nmComp);
                        ChangeLayerMask(uid, nmComp);
                    }
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
            if (npComp.TimeToCheck < curTime)
            {
                npComp.TimeToCheck = curTime + TimeSpan.FromSeconds(npComp.TimeBetweenChecksForShadowWalk);

                if (!CheckCanTransformToPolymorph(uid, npComp, xform))
                    _polymorphSystem.Revert((uid, peComp));
            }
        }
    }
}
