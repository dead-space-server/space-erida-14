using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Content.Shared.Examine;
using Content.Shared.Localizations;
using System.Linq;
using Robust.Shared.Toolshed.Commands.Values;

namespace Content.Shared.Weapons.Reflect;

/// <summary>
/// This handles reflecting projectiles and hitscan shots.
/// </summary>
public sealed partial class ReflectSystem : EntitySystem
{
    [Dependency] private INetManager _netManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private ItemToggleSystem _toggle = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.SubscribeWithRelay<ReflectComponent, ProjectileReflectAttemptEvent>(OnReflectUserCollide, baseEvent: false);
        Subs.SubscribeWithRelay<ReflectComponent, HitScanReflectAttemptEvent>(OnReflectUserHitscan, baseEvent: false);
        SubscribeLocalEvent<ReflectComponent, ProjectileReflectAttemptEvent>(OnReflectCollide);
        SubscribeLocalEvent<ReflectComponent, HitScanReflectAttemptEvent>(OnReflectHitscan);

        SubscribeLocalEvent<ReflectComponent, GotEquippedEvent>(OnReflectEquipped);
        SubscribeLocalEvent<ReflectComponent, GotUnequippedEvent>(OnReflectUnequipped);
        SubscribeLocalEvent<ReflectComponent, GotEquippedHandEvent>(OnReflectHandEquipped);
        SubscribeLocalEvent<ReflectComponent, GotUnequippedHandEvent>(OnReflectHandUnequipped);
        SubscribeLocalEvent<ReflectComponent, ExaminedEvent>(OnExamine);
    }

    private void OnReflectUserCollide(Entity<ReflectComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.InRightPlace)
            return; // only reflect when equipped correctly

        if (TryReflectProjectile(ent, ent.Owner, args.ProjUid))
            args.Cancelled = true;
    }

    private void OnReflectUserHitscan(Entity<ReflectComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected)
            return;

        if (!ent.Comp.InRightPlace)
            return; // only reflect when equipped correctly

        if (TryReflectHitscan(ent, ent.Owner, args.Shooter, args.SourceItem, args.Direction, args.Reflective, out var dir))
        {
            args.Direction = dir.Value;
            args.Reflected = true;
        }
    }

    private void OnReflectCollide(Entity<ReflectComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (TryReflectProjectile(ent, ent.Owner, args.ProjUid))
            args.Cancelled = true;
    }

    private void OnReflectHitscan(Entity<ReflectComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected)
            return;

        if (TryReflectHitscan(ent, ent.Owner, args.Shooter, args.SourceItem, args.Direction, args.Reflective, out var dir))
        {
            args.Direction = dir.Value;
            args.Reflected = true;
        }
    }

    private bool TryReflectProjectile(Entity<ReflectComponent> reflector, EntityUid user, Entity<ProjectileComponent?> projectile)
    {
        if (!TryComp<ReflectiveComponent>(projectile, out var reflective) ||
            !_toggle.IsActivated(reflector.Owner) ||
            !reflector.Comp.ReflectsProb.TryGetValue(reflective.Reflective, out var reflectProb) || // Erida-edit
            !_random.Prob(reflectProb) || // Erida-edit
            !TryComp<PhysicsComponent>(projectile, out var physics))
        {
            return false;
        }

        var rotation = _random.NextAngle(-reflector.Comp.Spread / 2, reflector.Comp.Spread / 2).Opposite();
        var existingVelocity = _physics.GetMapLinearVelocity(projectile, component: physics);
        var relativeVelocity = existingVelocity - _physics.GetMapLinearVelocity(user);
        var newVelocity = rotation.RotateVec(relativeVelocity);

        // Have the velocity in world terms above so need to convert it back to local.
        var difference = newVelocity - existingVelocity;

        _physics.SetLinearVelocity(projectile, physics.LinearVelocity + difference, body: physics);

        var locRot = Transform(projectile).LocalRotation;
        var newRot = rotation.RotateVec(locRot.ToVec());
        _transform.SetLocalRotation(projectile, newRot.ToAngle());

        PlayAudioAndPopup(reflector.Comp, user);

        if (Resolve(projectile, ref projectile.Comp, false))
        {
            _adminLogger.Add(LogType.BulletHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected {ToPrettyString(projectile)} from {ToPrettyString(projectile.Comp.Weapon)} shot by {projectile.Comp.Shooter}");

            projectile.Comp.Shooter = user;
            projectile.Comp.Weapon = user;
            Dirty(projectile, projectile.Comp);
        }
        else
        {
            _adminLogger.Add(LogType.BulletHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected {ToPrettyString(projectile)}");
        }

        return true;
    }
    private bool TryReflectHitscan(
        Entity<ReflectComponent> reflector,
        EntityUid user,
        EntityUid? shooter,
        EntityUid shotSource,
        Vector2 direction,
        ReflectType hitscanReflectType,
        [NotNullWhen(true)] out Vector2? newDirection)
    {
        if (!reflector.Comp.ReflectsProb.TryGetValue(hitscanReflectType, out var reflectProb) || // Eriad-edit
            !_toggle.IsActivated(reflector.Owner) ||
            !_random.Prob(reflectProb)) // Erida-edit
        {
            newDirection = null;
            return false;
        }

        PlayAudioAndPopup(reflector.Comp, user);

        var spread = _random.NextAngle(-reflector.Comp.Spread / 2, reflector.Comp.Spread / 2);
        newDirection = -spread.RotateVec(direction);

        if (shooter != null)
            _adminLogger.Add(LogType.HitScanHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected hitscan from {ToPrettyString(shotSource)} shot by {ToPrettyString(shooter.Value)}");
        else
            _adminLogger.Add(LogType.HitScanHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected hitscan from {ToPrettyString(shotSource)}");

        return true;
    }

    private void PlayAudioAndPopup(ReflectComponent reflect, EntityUid user)
    {
        // Can probably be changed for prediction
        if (_netManager.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("reflect-shot"), user);
            _audio.PlayPvs(reflect.SoundOnReflect, user);
        }
    }

    private void OnReflectEquipped(Entity<ReflectComponent> ent, ref GotEquippedEvent args)
    {
        ent.Comp.InRightPlace = (ent.Comp.SlotFlags & args.SlotFlags) == args.SlotFlags;
        Dirty(ent);
    }

    private void OnReflectUnequipped(Entity<ReflectComponent> ent, ref GotUnequippedEvent args)
    {
        ent.Comp.InRightPlace = false;
        Dirty(ent);
    }

    private void OnReflectHandEquipped(Entity<ReflectComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.InRightPlace = ent.Comp.ReflectingInHands;
        Dirty(ent);
    }

    private void OnReflectHandUnequipped(Entity<ReflectComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.InRightPlace = false;
        Dirty(ent);
    }

    #region Examine
    private float? SameProb(Dictionary<ReflectType, float> reflectsProb)
    {
        var probs = reflectsProb.Values.ToArray();
        var count = probs.Count();

        if (count < 2) return null;

        for (int i = 0; i < count - 1; i++)
        {
            if (probs[i] != probs[i + 1])
                return null;
        }

        return probs[0];
    }

    private void Examine(Entity<ReflectComponent> ent, float prob, ref ExaminedEvent args, ReflectType reflect = ReflectType.None)
    {
        var value = MathF.Round(prob * 100, 1);

        if (value == 0) // Erida-edit
            return;

        List<string> typeList = new();

        if (reflect == ReflectType.None)
        {
            var typeNames = ent.Comp.ReflectsProb.Keys.Select(t => t.ToString()).ToArray();

            foreach (var typeName in typeNames)
            {
                var type = Loc.GetString(("reflect-component-" + typeName).ToLower());
                typeList.Add(type);
            }
        }
        else
        {
            var type = Loc.GetString(("reflect-component-" + reflect.ToString()).ToLower());
            typeList.Add(type);
        }

        var msg = ContentLocalizationManager.FormatList(typeList);

        args.PushMarkup(Loc.GetString("reflect-component-examine", ("value", value), ("type", msg)));
    }

    private void OnExamine(Entity<ReflectComponent> ent, ref ExaminedEvent args)
    {
        // This isn't examine verb or something just because it looks too much bad.
        // Trust me, universal verb for the potential weapons, armor and walls looks awful.
        if (!_toggle.IsActivated(ent.Owner) || ent.Comp.ReflectsProb.Count == 0)
            return;

        var sProb = SameProb(ent.Comp.ReflectsProb);
        if (sProb.HasValue)
        {
            Examine(ent, sProb.Value, ref args);
            return;
        }

        foreach (var (type, prob) in ent.Comp.ReflectsProb)
        {
            Examine(ent, prob, ref args, type);
        }
    }
    #endregion
}
