using Content.Shared.Mobs;

namespace Content.Client.DamageState;

[RegisterComponent]
public sealed partial class DamageStateVisualsComponent : Component
{
    public int? OriginalDrawDepth;

    [DataField("states")] public Dictionary<MobState, Dictionary<DamageStateVisualLayers, string>> States = [];
}

public enum DamageStateVisualLayers : byte
{
    Base,
    BaseUnshaded,
}
