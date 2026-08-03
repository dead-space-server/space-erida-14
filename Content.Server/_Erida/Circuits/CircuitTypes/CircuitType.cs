using Content.Shared._Erida.Circuits;
using Content.Shared._Erida.Circuits.Components;
using Content.Shared.Verbs;

namespace Content.Server._Erida.Circuits;

public sealed partial class ServerCircuitSystem
{
    #region Activation
    private void ActivateCircuit(Entity<CircuitComponentComponent> ent)
    {
        switch (ent.Comp.AnswerType)
        {
            case CircuitResponseType.Button:
                {
                    OnSignalButton(ent);
                    break;
                }
            case CircuitResponseType.Voice:
                {
                    OnSignalVoice(ent);
                    break;
                }
            case CircuitResponseType.Test:
                {
                    break;
                }
            default:
                break;
        }
    }

    private void ActivateAllByType(Entity<CircuitSetupComponent> ent, CircuitResponseType type)
    {
        if (ent.Comp.InsertedParts.TryGetValue(type, out var components))
            foreach (var netComp in components)
            {
                var compEntity = GetEntity(netComp);
                if (TryComp<CircuitComponentComponent>(compEntity, out var comp))
                    PortSignalReceived((compEntity, comp));
            }
    }
    private void GetVerbsEvent(GetVerbsEvent<ExamineVerb> args, CircuitResponseType responseType, Entity<CircuitSetupComponent> ent)
    {
        switch (responseType)
        {
            case CircuitResponseType.Button:
                {
                    OnVerbsButton(args, responseType, ent);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
    #endregion

    #region Signals
    private void PortSignalReceived(Entity<CircuitComponentComponent> ent)
    {
        var activeSignals = 0;
        List<CircuitPortData> nullablePorts = [];

        for (var i = 0; i < ent.Comp.Inputs.Length; i++)
        {
            if (ent.Comp.Inputs[i].Data == null)
                nullablePorts.Add(ent.Comp.Inputs[i]);
            else
                if (ent.Comp.Inputs[i].DataType == CircuitDataFormat.Signal
                    && ent.Comp.Inputs[i].Data!.Value.Signal
                    && ent.Comp.Inputs[i].DataInvalidAt > _gameTiming.CurTime)
                    activeSignals += 1;
        }

        if (activeSignals < ent.Comp.NeedSignalsForActivate)
        {
            var component = EnsureComp<CircuitAwaitingSignalComponent>(ent.Owner);
            component.NullablePorts = nullablePorts;
            component.InactiveAt = _gameTiming.CurTime + component.SignalLiveTime;
        }
        else
            ActivateCircuit(ent);

    }
    private void SendSignalFromPort(CircuitPortData port)
    {
        var uid = GetEntity(port.ConnectedComponent!.Value);
        var comp = Comp<CircuitComponentComponent>(uid);

        SendSignalToPort((uid, comp), port.ConnectedIndex!.Value);
    }

    private void SendSignalToPort(Entity<CircuitComponentComponent> target, byte portIndex)
    {
        target.Comp.Inputs[portIndex].DataInvalidAt = _gameTiming.CurTime + target.Comp.Inputs[portIndex].DataLifeTime;
        target.Comp.Inputs[portIndex].Data = new CircuitData()
        {
            Signal = true
        };


        PortSignalReceived(target);
    }

    private void SendSignalFromAllPorts(CircuitComponentComponent component)
    {
        foreach (var port in component.Output)
        {
            if (port.DataType != CircuitDataFormat.Signal)
                continue;

            var uid = GetEntity(port.ConnectedComponent!.Value);
            var comp = Comp<CircuitComponentComponent>(uid);
            comp.Inputs[port.ConnectedIndex!.Value].DataInvalidAt = _gameTiming.CurTime + comp.Inputs[port.ConnectedIndex!.Value].DataLifeTime;
            comp.Inputs[port.ConnectedIndex!.Value].Data = new CircuitData()
            {
                Signal = true
            };

            PortSignalReceived((uid, comp));
        }
    }
    #endregion

    #region Data
    private void SendDataFromPort(ref CircuitPortData port, ref CircuitData data)
    {
        var uid = GetEntity(port.ConnectedComponent!.Value);
        var comp = Comp<CircuitComponentComponent>(uid);

        SendDatalToPort(comp, port.ConnectedIndex!.Value, ref data);
    }

    private void SendDatalToPort(CircuitComponentComponent target, byte portIndex, ref CircuitData data)
    {
        target.Inputs[portIndex].DataInvalidAt = _gameTiming.CurTime + target.Inputs[portIndex].DataLifeTime;
        target.Inputs[portIndex].Data = data;
    }

    private bool EnsureAllPortsHaveValue(List<CircuitPortData> ports, EntityUid entityUid)
    {
        foreach (var port in ports)
            if (port.Data != null && port.DataInvalidAt > _gameTiming.CurTime)
                ports.Remove(port);

        if (ports.Count == 0)
            return true;

        var comp = EnsureComp<CircuitAwaitingSignalComponent>(entityUid);
        comp.NullablePorts = ports;
        comp.InactiveAt = _gameTiming.CurTime + comp.SignalLiveTime;
        return false;
    }
    #endregion

    #region Power
    private bool ConsumePowerIfEnough(NetEntity? battery, float charge)
    {
        if (!TryGetEntity(battery, out var entityUid))
            return false;

        if (!_batterySystem.TryUseCharge((entityUid.Value, null), charge))
            return false;

        return true;
    }
    #endregion

}

