using Robust.Shared.GameStates;
using Content.Shared._ADT.Silicon.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared._ADT.Silicon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SiliconComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public short ChargeState = 10;

    [ViewVariables(VVAccess.ReadOnly)]
    public float OverheatAccumulator = 0.0f;

    public TimeSpan LastDrainTime = TimeSpan.Zero;

    public bool Dead = false;

    [DataField(customTypeSerializer: typeof(EnumSerializer))]
    public Enum EntityType = SiliconType.Npc;

    [DataField]
    public bool BatteryPowered = false;

    [DataField]
    public float DrainPerSecond = 50f;

    [DataField]
    public float IdleDrainReduction = 0.6f;

    [DataField]
    public float? ChargeThresholdMid = 0.5f;

    [DataField]
    public float? ChargeThresholdLow = 0.25f;

    [DataField]
    public float? ChargeThresholdCritical = 0.1f;

    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField(required: true)]
    public Dictionary<int, float> SpeedModifierThresholds = default!;

    [DataField]
    public float FireStackMultiplier = 1f;

    [DataField]
    public bool DoSiliconsDreamOfElectricSheep;
}
