
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Power;

namespace Content.Server._Erida.AirtightRequiresPower;

public sealed class AirtightRequiresPowerSystem : EntitySystem
{
    [Dependency] private readonly AirtightSystem _airtightSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AirtightRequiresPowerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AirtightRequiresPowerComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnMapInit(EntityUid uid, AirtightRequiresPowerComponent component, ref MapInitEvent args)
    {
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var apcPowerReceiver)) return;

        ChangeAirblocked(uid, apcPowerReceiver.Powered);
    }

    private void OnPowerChanged(EntityUid uid, AirtightRequiresPowerComponent component, ref PowerChangedEvent args)
    {
        ChangeAirblocked(uid, args.Powered);
    }

    private void ChangeAirblocked(EntityUid uid, bool state)
    {
        if (!TryComp<AirtightComponent>(uid, out var comp)) return;

        if (state)
            _airtightSystem.SetAirblocked((uid, comp), true);
        else
            _airtightSystem.SetAirblocked((uid, comp), false);
    }
}
