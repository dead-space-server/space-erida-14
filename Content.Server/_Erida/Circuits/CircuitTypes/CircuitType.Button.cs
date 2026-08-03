using System.Linq;
using Content.Shared._Erida.Circuits;
using Content.Shared._Erida.Circuits.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server._Erida.Circuits;

public sealed partial class ServerCircuitSystem
{
    private void OnSignalButton(Entity<CircuitComponentComponent> ent)
    {
        if (!ConsumePowerIfEnough(ent.Comp.NetContainer, ent.Comp.PowerConsuming))
            return;

        SendSignalFromPort(ent.Comp.Output[0]);
    }

    private void OnVerbsButton(GetVerbsEvent<ExamineVerb> args, CircuitResponseType responseType, Entity<CircuitSetupComponent> ent)
    {
        var verb = new ExamineVerb
        {
            Act = () =>
            {
                ActivateAllByType(ent, responseType);
            },
            Text = Loc.GetString("verb-text-integrated-circuit-button"),
            Category = VerbCategory.IntegratedCircuit,
        };

        args.Verbs.Add(verb);
    }
}
