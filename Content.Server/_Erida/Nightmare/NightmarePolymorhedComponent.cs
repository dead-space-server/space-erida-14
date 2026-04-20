using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.Nightmare.Components;

[RegisterComponent, Access(typeof(NightmareSystem))]
public sealed partial class NightmarePolymorhepComponent : Component
{
    [DataField]
    public float timeBetweenChecks = 0.5f;

    public TimeSpan timeToCheck = TimeSpan.Zero;

    [DataField]
    public float redLineOfLight = 0.01f;
}
