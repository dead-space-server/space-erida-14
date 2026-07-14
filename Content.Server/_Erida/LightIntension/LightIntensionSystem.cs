using System.Linq;
using System.Runtime.CompilerServices;
using Content.Server._Erida.LightBehaviourServer;
using Content.Server.Light.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Physics;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.LightIntension;

public sealed partial class LightIntensionSystem : EntitySystem
{
    [Dependency] private EntityManager _entityManager = default!;
    [Dependency] private PhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transformSystem = default!;
    [Dependency] private ContainerSystem _containerSystem = default!;
    [Dependency] private LightBehaviourServerSystem _behaviourSystem = default!;
    [Dependency] private EntityLookupSystem _entityLookupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public float TryGetLightLevel(Entity<TransformComponent> ent, float? maxCap = null)
    {
        float totalIlluminance = 0;

        if (_containerSystem.TryGetContainingContainer(ent.Owner, out var container))
        {
            foreach (var item in container.ContainedEntities)
                if (TryComp<PointLightComponent>(item, out var plComp)
                    && plComp.Enabled)
                {
                    totalIlluminance += plComp.Energy;

                    if (maxCap != null
                        && maxCap < totalIlluminance)
                        return maxCap.Value;
                }
            return totalIlluminance;
        }

        var entMapCoordsVector2d = _transformSystem.ToMapCoordinates(ent.Comp.Coordinates).Position;

        var query = _entityLookupSystem.GetEntitiesInRange<PointLightComponent>(_transformSystem.GetMapCoordinates(ent, ent.Comp), 1f);
        foreach (var uid in query)
        {
            var lightComp = uid.Comp;

            var xform = _entityManager.GetComponent<TransformComponent>(uid);

            TryComp<ExpendableLightComponent>(uid, out var elComp);
            TryComp<LightBehaviourServerComponent>(uid, out var lbsComp);

            if (!lightComp.Enabled
                && (elComp == null || elComp.Activated == false
                || lbsComp == null))
                continue;

            if (!ent.Comp.Coordinates.TryDistance(_entityManager, xform.Coordinates, out var distance))
                continue;

            float lightRadius;

            if (lbsComp != null)
            {
                _behaviourSystem.TryGetRadius(uid, out var lbsRadius, lbsComp);

                if (lbsRadius == null
                    || distance > lbsRadius)
                    continue;

                lightRadius = lbsRadius.Value;
            }
            else
            {
                if (distance > lightComp.Radius)
                    continue;

                lightRadius = lightComp.Radius;
            }

            if (TryComp<EyeComponent>(ent, out var eComp))
            {
                if (TryComp<VisibilityComponent>(uid, out var vComp)
                    && vComp.Layer != eComp.VisibilityMask)
                    continue;

                if (TryComp<VisibilityComponent>(Transform(uid).ParentUid, out var vParentComp)
                    && vParentComp.Layer != eComp.VisibilityMask)
                    continue;
            }

            if (HasComp<ClothingComponent>(Transform(uid).ParentUid))
                continue;

            var lightPointMapCoordsVector2d = _transformSystem.ToMapCoordinates(xform.Coordinates).Position;

            var direction = lightPointMapCoordsVector2d - entMapCoordsVector2d;

            if (!direction.IsValid()
                || direction.IsLengthZero())
                continue;

            direction = direction.Normalized();

            var mask = (int)CollisionGroup.Opaque;
            var ray = new CollisionRay(entMapCoordsVector2d, direction, mask);
            var results = _physics.IntersectRay(ent.Comp.MapID, ray, distance, null, false);

            if (results.Any(r => HasComp<OccluderComponent>(r.HitEntity)))
                continue;

            if (lightComp.MaskPath is { } maskPath)
            {
                var relative = direction * -1;
                var rotation = _transformSystem.GetWorldRotation(uid);

                var local = (-rotation).RotateVec(relative);

                var x = local.X;
                var y = local.Y;

                if (maskPath.EndsWith("cone.png"))
                {
                    if (-y < x * x * 0.25f - 0.5f)
                        continue;
                }
                else if (maskPath.EndsWith("double_cone.png"))
                {
                    var cond1 = y >= x * x * 0.25f - 0.5f;
                    var cond2 = -y >= x * x * 0.25f - 0.5f;

                    if (!cond1 && !cond2)
                        continue;
                }
            }

            var normalizedDist = distance / lightRadius;
            var attenuation = MathF.Pow(1 - normalizedDist, lightComp.Falloff);
            totalIlluminance += lightComp.Energy * attenuation;

            if (maxCap != null
                && maxCap < totalIlluminance)
                return maxCap.Value;
        }

        return totalIlluminance;
    }
}
