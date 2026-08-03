namespace Content.Shared._Erida.Circuits.Components;

[RegisterComponent]
public sealed partial class CircuitAwaitingSignalComponent : Component
{
    public List<CircuitPortData> NullablePorts = [];

    public TimeSpan InactiveAt = TimeSpan.Zero;

    public TimeSpan SignalLiveTime = TimeSpan.FromSeconds(2);

    public CircuitComponentComponent? CircuitComponentComponent = null;
}
