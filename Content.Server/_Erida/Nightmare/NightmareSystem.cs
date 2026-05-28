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
using Content.Shared.Alert;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Flash;
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

public sealed partial class NightmareSystem : SharedNightmareSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private LightIntensionSystem _lightIntension = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private PolymorphSystem _polymorphSystem = default!;
    [Dependency] private SharedActionsSystem _action = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private MapSystem _mapSystem = default!;
    [Dependency] private MobThresholdSystem _mobThreshold = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private EntityManager _entityManager = default!;
    [Dependency] private RoleSystem _role = default!;
    [Dependency] private MindSystem _mindSystem = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private FixtureSystem _fixture = default!;
    [Dependency] private PhysicsSystem _physicsSystem = default!;
    [Dependency] private AlertsSystem _alert = default!;

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

        SubscribeLocalEvent<NightmareComponent, AfterFlashedEvent>(OnGetFlashed);
    }

    private void OnInit(EntityUid uid, NightmareComponent component, MapInitEvent args)
    {
        if (!HasComp<NightmarePolymorhedComponent>(uid))
        {
            _action.AddAction(uid, ref component.ShadowWalkActionEntity, component.ShadowWalkAction);

            if (!TryComp<FixturesComponent>(uid, out var fComp))
                return;

            var currFixture = _fixture.GetFixtureOrNull(uid, "fix1", fComp);

            if (currFixture != null)
                component.OldLayer = currFixture.CollisionLayer;

            var xform = Comp<TransformComponent>(uid);

            if (xform == null)
                return;

            var lightIntension = _lightIntension.TryGetLightLevel((uid, xform), component.MaxLightCap);
            if (lightIntension <= component.RedLineOfLight)
            {
                ChangeLayer(uid, component.NewLayer);
                component.InTheDark = true;
                _alert.ShowAlert(uid, component.Alert);
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

    private void OnGetFlashed(Entity<NightmareComponent> entity, ref AfterFlashedEvent args)
    {
        _damageable.TryChangeDamage(entity.Owner, entity.Comp.DamageFromGetFlashed, true);
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

    private bool CheckCanTransformToPolymorph(EntityUid uid, NightmareComponent nmComp, TransformComponent xform)
    {
        var gridUid = Transform(uid).GridUid;

        if (gridUid == null
            || !TryComp<MapGridComponent>(gridUid, out var grid)
            || _turf.IsSpace(_mapSystem.GetTileRef(gridUid.Value, grid, Transform(uid).Coordinates)))
        {
            return false;
        }

        var lightIntension = _lightIntension.TryGetLightLevel((uid, xform), nmComp.MaxLightCap);

        if (lightIntension > nmComp.RedLineOfLight)
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
            Start(uid);
        }
    }

    private void OnMindRemoved(EntityUid uid, NightmareComponent component, MindRemovedMessage args)
    {
        if (_role.MindHasRole<NightmareRoleComponent>(args.Mind.Owner, out var nRole)
        && nRole.Value.Comp2.PolymorphState)
            return;

        _role.MindRemoveRole<NightmareRoleComponent>(args.Mind.Owner);
    }

    public void Start(EntityUid uid)
    {
        if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind))
        {
            return;
        }

        _role.MindAddRole(mindId, "MindRoleNightmare");

        if (mind is { UserId: not null } && _player.TryGetSessionById(mind.UserId, out var session))
            _antag.SendBriefing(session, Loc.GetString("nightmare-role-greeting"), null, null);
    }

    private void ChangeLayer(EntityUid uid, int? layer)
    {
        if (!TryComp<FixturesComponent>(uid, out var fComp))
            return;

        if (layer == null)
            return;

        var currFixture = _fixture.GetFixtureOrNull(uid, "fix1", fComp);

        if (currFixture == null)
            return;

        _physicsSystem.SetCollisionLayer(uid, "fix1", currFixture, layer.Value);
    }
    private void UpdateDarkState(EntityUid uid, NightmareComponent nmComp, bool ligth)
    {
        if (ligth)
        {
            nmComp.InTheDark = false;

            ChangeLayer(uid, nmComp.OldLayer);
            _alert.ClearAlert(uid, nmComp.Alert);
        }
        else
        {
            nmComp.InTheDark = true;

            ChangeLayer(uid, nmComp.NewLayer);
            _alert.ShowAlert(uid, nmComp.Alert);
        }
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

                var lightIntension = _lightIntension.TryGetLightLevel((uid, xform), nmComp.MaxLightCap);
                if (lightIntension > nmComp.RedLineOfLight)
                {
                    UpdateDarkState(uid, nmComp, true);

                    var scale = lightIntension - nmComp.RedLineOfLight;
                    _damageable.TryChangeDamage(uid, nmComp.DamageFromBurn * scale, true, false);
                    if (nmComp.PlayAudio)
                        _audio.PlayPvs(nmComp.BurnSound, uid);
                }
                else
                {
                    _damageable.TryChangeDamage(uid, nmComp.HealthFromDarkness, true, false);

                    UpdateDarkState(uid, nmComp, false);
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
