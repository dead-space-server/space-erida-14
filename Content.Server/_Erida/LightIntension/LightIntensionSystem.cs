using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.LightIntension;

public sealed class LightIntensionSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public float TryGetLightLevel(Entity<TransformComponent> ent)
    {
        float totalIlluminance = 0;

        var query = EntityQueryEnumerator<PointLightComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var lightComp, out var xform))
        {
            if (!lightComp.Enabled)
                continue;

            if (!ent.Comp.Coordinates.TryDistance(_entityManager, xform.Coordinates, out var distance)
                || distance > lightComp.Radius)
                continue;

            var entMapCoordsVector2d = _transformSystem.ToMapCoordinates(ent.Comp.Coordinates).Position;
            var lightPointMapCoordsVector2d = _transformSystem.ToMapCoordinates(xform.Coordinates).Position;

            var direction = lightPointMapCoordsVector2d - entMapCoordsVector2d;

            //Logger.Debug($"coordsPos: {ent.Comp.Coordinates.Position}; x, y: {ent.Comp.Coordinates.X}, {ent.Comp.Coordinates.Y}");
            //Logger.Debug($"lightUid: {uid}, coords: {_transformSystem.ToMapCoordinates(ent.Comp.Coordinates).Position}, {_transformSystem.ToMapCoordinates(xform.Coordinates).Position}, {direction}");

            if (!direction.IsValid()
                || direction.IsLengthZero())
                continue;

            direction = direction.Normalized();

            var mask = (int)CollisionGroup.Opaque;
            var ray = new CollisionRay(entMapCoordsVector2d, direction, mask); // ent.Comp.Coordinates.Position xform.Coordinates.Position
            var results = _physics.IntersectRay(ent.Comp.MapID, ray, distance, null, false);
            foreach (var a in results)
            {
                Logger.Debug($"lightUid : item {uid} : {a.HitEntity}");
            }
            if (results.Any(r => HasComp<OccluderComponent>(r.HitEntity)))
                continue;

            Logger.Debug($"uid that give some shit: {uid}");

            var normalizedDist = distance / lightComp.Radius;
            var attenuation = MathF.Pow(1 - normalizedDist, lightComp.Falloff);
            totalIlluminance += lightComp.Energy * attenuation;
        }

        return totalIlluminance;
    }
}
