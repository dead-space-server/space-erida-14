using Content.Server.Power.EntitySystems;
using Content.Shared._Erida.Circuits;
using Content.Shared._Erida.Circuits.Components;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._Erida.Circuits;

public sealed partial class ServerCircuitSystem : EntitySystem
{
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private BatterySystem _batterySystem = default!;
    [Dependency] private ContainerSystem _containerSystem = default!;
    [Dependency] private IEntityManager _entityManager = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CircuitComponentComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<CircuitSetupComponent, CircuitComponentActivated>(OnActivateComponent);

        SubscribeLocalEvent<CircuitSetupComponent, CircuitDeleteComponentMessage>(OnDeleteComponentRequest);

        SubscribeLocalEvent<CircuitSetupComponent, CircuitCreateLinkMessage>(OnCreateLinkRequest);
        SubscribeLocalEvent<CircuitSetupComponent, CircuitDeleteLinkMessage>(OnDeleteLinkRequest);

        SubscribeLocalEvent<CircuitSetupComponent, GetVerbsEvent<ExamineVerb>>(OnSetupGetVerb);

        // TODO add on comp destroyed
    }

    #region Events
    private void OnComponentInit(Entity<CircuitComponentComponent> ent, ref ComponentInit args)
    {
        for (var i = 0; i < ent.Comp.Output.Length; i++)
            ent.Comp.Output[i].IsOutput = true;
    }

    private void OnActivateComponent(Entity<CircuitSetupComponent> ent, ref CircuitComponentActivated args)
    {
        if (args.Component == null)
            return;

        ActivateCircuit(args.Component.Value);
    }

    private void OnDeleteComponentRequest(Entity<CircuitSetupComponent> ent, ref CircuitDeleteComponentMessage args)
    {
        // TODO добавить удаление данных при разрыве соединения
        var entityUid = GetEntity(args.CompEntity);

        if (!TryComp<CircuitComponentComponent>(entityUid, out var comp))
            return;

        for (var i = 0; i < comp.Inputs.Length; i++)
            EnsurePortConnectionEmpty(ref comp.Inputs[i]);


        for (var i = 0; i < comp.Output.Length; i++)
            EnsurePortConnectionEmpty(ref comp.Output[i]);

        if (!_containerSystem.TryRemoveFromContainer(entityUid))
            return;

        Dirty(ent);

        comp.NetContainer = null;
    }

    private void OnCreateLinkRequest(Entity<CircuitSetupComponent> ent, ref CircuitCreateLinkMessage args)
    {
        var input = GetEntity(args.LinkData.InputComponent);
        var output = GetEntity(args.LinkData.OutputComponent);

        if (!TryComp<CircuitComponentComponent>(output, out var outputComp)
            || !TryComp<CircuitComponentComponent>(input, out var inputComp))
            return;

        var data = new CircuitLinkData
        {
            OutputComponent = (output, outputComp),
            OutputPortIndex = args.LinkData.OutputPortIndex,

            InputComponent = (input, inputComp),
            InputPortIndex = args.LinkData.InputPortIndex,
        };

        TryLinkPorts(ref data);
    }

    private void OnDeleteLinkRequest(Entity<CircuitSetupComponent> ent, ref CircuitDeleteLinkMessage args)
    {
        EnsurePortConnectionEmpty(ref args.PortData);
    }

    private void OnSetupGetVerb(Entity<CircuitSetupComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!ent.Comp.EventParts.TryGetValue(CircuitEventType.Verb, out var value))
            return;

        foreach (var type in value)
            GetVerbsEvent(args, type, ent);
    }
    #endregion

    #region Ports
    public bool TryLinkPorts(ref CircuitLinkData data)
    {
        if (data.InputComponent.Owner == data.OutputComponent.Owner)
            return false;

        if (data.InputComponent.Comp.Inputs.Length - 1 < data.InputPortIndex
            || data.OutputComponent.Comp.Output.Length - 1 < data.OutputPortIndex)
            return false;

        if (data.InputComponent.Comp.Inputs[data.InputPortIndex].DataType
            != data.OutputComponent.Comp.Output[data.OutputPortIndex].DataType)
            return false;

        LinkPorts(ref data);
        return true;
    }

    private void LinkPorts(ref CircuitLinkData data)
    {
        ref var inputPort = ref data.InputComponent.Comp.Inputs[data.InputPortIndex];
        ref var outputPort = ref data.OutputComponent.Comp.Output[data.OutputPortIndex];

        DisconnectPorts(ref data);

        inputPort.ConnectedIndex = data.OutputPortIndex;
        inputPort.ConnectedComponent = GetNetEntity(data.OutputComponent.Owner);

        outputPort.ConnectedIndex = data.InputPortIndex;
        outputPort.ConnectedComponent = GetNetEntity(data.InputComponent.Owner);

        Dirty(data.InputComponent);
        Dirty(data.OutputComponent);
    }

    public void DisconnectPorts(ref CircuitLinkData data)
    {
        EnsurePortConnectionEmpty(ref data.InputComponent.Comp.Inputs[data.InputPortIndex]);
        EnsurePortConnectionEmpty(ref data.OutputComponent.Comp.Output[data.OutputPortIndex]);
    }

    public bool EnsurePortConnectionEmpty(ref CircuitPortData data)
    {
        if (data.ConnectedIndex == null && data.ConnectedComponent == null)
            return false;

        if (data.ConnectedIndex == null || data.ConnectedComponent == null)
        {
            ForceNullConnection(ref data);
            return true;
        }

        if (!TryGetEntity(data.ConnectedComponent, out var otherUid)
            || !TryComp<CircuitComponentComponent>(otherUid, out var otherComp))
        {
            ForceNullConnection(ref data);
            return true;
        }

        var container = !data.IsOutput ? otherComp.Output : otherComp.Inputs;

        if (data.ConnectedIndex >= container.Length)
        {
            ForceNullConnection(ref data);
            return true;
        }


        var ourUid = GetEntity(container[data.ConnectedIndex.Value].ConnectedComponent);
        TryComp<CircuitComponentComponent>(ourUid, out var ourComp);


        ForceNullConnection(ref container[data.ConnectedIndex.Value]);
        Dirty(otherUid.Value, otherComp);

        // We should delete connection in data only after deleting in connected component
        if (ourUid != null && ourComp != null)
        {
            ForceNullConnection(ref data);
            Dirty(ourUid.Value, ourComp);
        }
        return true;
    }

    private void ForceNullConnection(ref CircuitPortData data)
    {
        data.ConnectedComponent = null;
        data.ConnectedIndex = null;
    }
    #endregion

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = AllEntityQuery<CircuitAwaitingSignalComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.InactiveAt < _gameTiming.CurTime)
            {
                if (TryComp<CircuitComponentComponent>(uid, out var componentComponent))
                    for (var i = 0; i < componentComponent.Inputs.Length - 1; i++)
                        if (componentComponent.Inputs[i].ShouldDataDeleted)
                            componentComponent.Inputs[i].Data = null;

                RemComp(uid, comp);
            }
            else
            {
                foreach (var data in comp.NullablePorts)
                {
                    if (data.Data != null)
                        comp.NullablePorts.Remove(data);
                }

                if (comp.NullablePorts.Count == 0 && TryComp<CircuitComponentComponent>(uid, out var componentComponent))
                {
                    ActivateCircuit((uid, componentComponent));
                    RemComp(uid, comp);
                }
            }
        }
    }
}

