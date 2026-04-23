using System.Runtime.CompilerServices;
using Content.Server._Erida.LightDestroyer.Components;
using Content.Shared._Erida.LightDestroyer;
using Content.Shared._Erida.LightDestroyer.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.LightDestroyer;

public sealed class LightDestroyerSystem : SharedLightDestroyerSystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightDestroyerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<LightDestroyerComponent, ContactInteractionEvent>(OnContanctInteract);
        SubscribeLocalEvent<DestroyedByLightDestroyerComponent, ComponentInit>(OnDestroyedInit);
    }

    public void OnMeleeHit(Entity<LightDestroyerComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var item in args.HitEntities)
        {
            TryToFindAndDestroyLight(ent, item);
        }
    }

    public void OnContanctInteract(Entity<LightDestroyerComponent> ent, ref ContactInteractionEvent args)
    {
        TryToFindAndDestroyLight(ent, args.Other);
    }

    private void TryToFindAndDestroyLight(Entity<LightDestroyerComponent> ent, EntityUid? entity)
    {
        if (entity == null)
            return;

        TryToDestroyLight(ent, entity);

        var handsCheck = TryComp<HandsComponent>(entity, out var handsComp);
        var invCheck = TryComp<InventoryComponent>(entity, out var invComp);

        if (handsCheck || invCheck)
        {
            var inv = _inventorySystem.GetHandOrInventoryEntities((entity.Value, handsComp, invComp));

            foreach (var slot in inv)
                TryToDestroyLight(ent, slot);
        }
    }

    private void TryToDestroyLight(Entity<LightDestroyerComponent> ent, EntityUid? entity)
    {
        if (entity == null)
            return;

        if (CheckLight(entity))
        {
            if (HasComp<DestroyableByLightDestroyerComponent>(entity))
            {
                Destroy(entity, ent.Comp.Entity, ent.Comp.Sound);
                return;
            }

            EnsureComp<DestroyedByLightDestroyerComponent>(entity.Value);
        }
    }

    private bool CheckLight(EntityUid? entity)
    {
        if (TryComp<PointLightComponent>(entity, out var comp)
            && comp.Enabled)
            return true;

        return false;
    }

    private void Destroy(EntityUid? ent, EntProtoId? entToSpawn, SoundSpecifier? soundToPlay)
    {
        if (ent == null)
            return;

        if (entToSpawn != null)
            Spawn(entToSpawn, Transform(ent.Value).Coordinates);

        if (soundToPlay != null)
            _audio.PlayPvs(soundToPlay, ent.Value);

        QueueDel(ent);
    }

    private void OnDestroyedInit(Entity<DestroyedByLightDestroyerComponent> ent, ref ComponentInit args)
    {
        ent.Comp.TimeToDestroy = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.TimeNeedToDestroy);
        _pointLight.SetEnabled(ent, false);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<DestroyedByLightDestroyerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.TimeToDestroy < curTime)
                RemComp(uid, comp);
        }
    }
}
