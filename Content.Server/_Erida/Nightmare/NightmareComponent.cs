namespace Content.Server._Erida.Nightmare.Components;

[RegisterComponent, Access(typeof(NightmareSystem))]
public sealed partial class NightmareComponent : Component
{
    [DataField]
    public float timeBetweenChecks = 0.5f;

    public TimeSpan timeToCheck = TimeSpan.Zero;
}
