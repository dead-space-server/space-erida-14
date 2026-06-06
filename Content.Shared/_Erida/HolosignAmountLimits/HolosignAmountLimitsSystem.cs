using Content.Shared._Erida.HolosignAmountLimits.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Storage;
using Robust.Shared.Network;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.HolosignAmountLimits;

public sealed partial class HolosignAmountLimitsSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private EntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolosignAmountLimitsComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
        SubscribeLocalEvent<HolosignAmountLimitsComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<HolosignAmountLimitsComponent, UseInHandEvent>(OnHandUse);
        SubscribeLocalEvent<HolosignAmountLimitsSignComponent, ComponentRemove>(OnRemove);
    }

    private void OnExamine(Entity<HolosignAmountLimitsComponent> ent, ref ExaminedEvent args)
    {
        var charges = ent.Comp.MaxAmount - ent.Comp.CurrentAmount;

        using (args.PushGroup(nameof(HolosignAmountLimitsComponent)))
        {
            args.PushMarkup(Loc.GetString("limited-charges-charges-remaining", ("charges", charges)));
        }
    }

    private void OnBeforeInteract(Entity<HolosignAmountLimitsComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled
            || !args.CanReach // prevent placing out of range
            || HasComp<StorageComponent>(args.Target) // if it's a storage component like a bag, we ignore usage so it can be stored
            )
            return;

        if (ent.Comp.CurrentAmount > 0
            && TryComp<HolosignAmountLimitsSignComponent>(args.Target, out var haComp)
            && haComp.Overlord == ent.Owner)
        {
            _entityManager.DeleteEntity(args.Target);
            args.Handled = true;
            return;
        }

        if (ent.Comp.MaxAmount == ent.Comp.CurrentAmount) // if no amounts left, doesn't work
            return;

        // overlapping of the same holo on one tile remains allowed to allow holofan refreshes
        if (ent.Comp.PredictedSpawn || _net.IsServer)
        {
            var holosign = PredictedSpawnAtPosition(ent.Comp.SignProto, args.ClickLocation);
            Transform(holosign).LocalRotation = Angle.Zero;
            var comp = EnsureComp<HolosignAmountLimitsSignComponent>(holosign);
            comp.Overlord = ent;
            ent.Comp.CurrentAmount++;
            ent.Comp.SpawnedSigns.Add(holosign);
        }

        args.Handled = true;
    }

    private void OnHandUse(Entity<HolosignAmountLimitsComponent> ent, ref UseInHandEvent args)
    {
        foreach (var sign in ent.Comp.SpawnedSigns)
        {
            _entityManager.DeleteEntity(sign);
        }
    }

    private void OnRemove(Entity<HolosignAmountLimitsSignComponent> ent, ref ComponentRemove args)
    {
        if (TryComp<HolosignAmountLimitsComponent>(ent.Comp.Overlord, out var haComp))
        {
            haComp.SpawnedSigns.Remove(ent.Owner);
            haComp.CurrentAmount--;
        }
    }
}
