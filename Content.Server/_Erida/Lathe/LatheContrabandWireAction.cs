using Content.Server.Lathe;
using Content.Server.Wires;
using Content.Shared._Erida.Lathe;
using Content.Shared.Lathe;
using Content.Shared.Wires;

namespace Content.Server._Erida.Lathe;

[DataDefinition]
public sealed partial class LatheContrabandWireAction : BaseToggleWireAction
{
    private LatheSystem _latheSystem = default!;

    public override Color Color { get; set; } = Color.Green;
    public override string Name { get; set; } = "wire-name-vending-contraband";
    public override object? StatusKey { get; } = ContrabandLatheWireKey.StatusKey;
    public override object? TimeoutKey { get; } = ContrabandLatheWireKey.TimeoutKey;

    public override void Initialize()
    {
        base.Initialize();

        _latheSystem = EntityManager.System<LatheSystem>();
    }

    public override StatusLightState? GetLightState(Wire wire)
    {
        if (EntityManager.TryGetComponent(wire.Owner, out LatheComponent? lathe))
        {
            return lathe.Contraband
                ? StatusLightState.BlinkingSlow
                : StatusLightState.On;
        }

        return StatusLightState.Off;
    }

    public override void ToggleValue(EntityUid owner, bool setting)
    {
        if (EntityManager.TryGetComponent(owner, out LatheComponent? lathe))
        {
            _latheSystem.SetContraband(owner, !lathe.Contraband, lathe);
        }
    }
    public override bool GetValue(EntityUid owner)
    {
        return EntityManager.TryGetComponent(owner, out LatheComponent? lathe) && !lathe.Contraband;
    }
}
