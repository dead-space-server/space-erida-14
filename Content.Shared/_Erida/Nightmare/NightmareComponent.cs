using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.Nightmare.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedNightmareSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class NightmareComponent : Component
{
    [DataField]
    public float timeBetweenChecks = 0.5f;

    [DataField]
    public float timeBetweenChecksForShadowWalk = 0.05f;

    public TimeSpan timeToCheck = TimeSpan.Zero;

    [DataField]
    public float redLineOfLight = 0.01f;

    [DataField]
    public DamageSpecifier damageFromBurn = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Heat", 20 },
        },
    };

    [DataField]
    public DamageSpecifier healthFromDarkness = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Blunt", -2.5 },
            { "Slash", -2.5 },
            { "Piercing", -2.5 },
            { "Heat", -2.5 },
            { "Shock", -2.5 },
        },
    };

    [DataField]
    public SoundSpecifier BurnSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField]
    public EntProtoId ShadowWalkAction = "ActionShadowWalk";

    [DataField, AutoNetworkedField]
    public EntityUid? ShadowWalkActionEntity;
}
