using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.Nightmare.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedNightmareSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class NightmareComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool InTheDark = false;

    public int? OldLayer;

    public int NewLayer = (int)CollisionGroup.Opaque;

    [DataField]
    public float TimeBetweenChecks = 0.5f;

    [DataField]
    public float TimeBetweenChecksForShadowWalk = 0.05f;

    public TimeSpan TimeToCheck = TimeSpan.Zero;

    [DataField]
    public float RedLineOfLight = 0.0085f;

    [DataField]
    public float MaxLightCap = 1f;

    [DataField]
    public DamageSpecifier DamageFromBurn = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Heat", 15 },
        },
    };

    [DataField]
    public FixedPoint2 MaxDamageFromBurn = 200;

    [DataField]
    public DamageSpecifier DamageFromGetFlashed = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Heat", 30 },
        },
    };

    [DataField]
    public DamageSpecifier HealthFromDarkness = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Blunt", -1.25 },
            { "Slash", -1.25 },
            { "Piercing", -1.25 },

            { "Heat", -1.25 },
            { "Shock", -1.25 },
            { "Cold", -1.25 },
            { "Caustic", -1.25 },

            { "Poison", -1.25 },
            { "Radiation", -1.25 },

            { "Asphyxiation", -1.25 },
            { "Bloodloss", -1.25 },
        },
    };

    [DataField]
    public bool PlayAudio = false;

    [DataField]
    public SoundSpecifier BurnSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField]
    public EntProtoId ShadowWalkAction = "ActionShadowWalk";

    [DataField, AutoNetworkedField]
    public EntityUid? ShadowWalkActionEntity;

    [DataField]
    public ProtoId<AlertPrototype> Alert = "InShade";
}
