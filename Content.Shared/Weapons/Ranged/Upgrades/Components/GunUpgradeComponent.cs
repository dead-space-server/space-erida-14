using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// Used to denote compatibility with <see cref="UpgradeableGunComponent"/>. Does not contain explicit behavior.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(GunUpgradeSystem), typeof(Content.Shared._Lavaland.Weapons.Ranged.Upgrades.SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeComponent : Component
{
    /// <summary>
    /// Literal name of this upgrade that is shown on Lavaland examine texts.
    /// </summary>
    [DataField]
    public LocId? Name;

    /// <summary>
    /// Text to use when examining the upgrade itself through Lavaland upgrade slots.
    /// </summary>
    [DataField]
    public LocId? ExamineTextType = "gun-upgrade-examine-type-upgrade";

    /// <summary>
    /// Text template to use when examining a weapon where this upgrade is inserted through Lavaland upgrade slots.
    /// </summary>
    [DataField]
    public LocId? InsertedTextType = "gun-upgrade-inserted-examine-type-contains";

    [DataField]
    public int? CapacityCost;

    /// <summary>
    /// If this string matches with some other Lavaland weapon upgrade, it will fail to install.
    /// </summary>
    [DataField]
    public string? UniqueGroup;

    /// <summary>
    /// Tags used to ensure mutually exclusive upgrades and duplicates are not stacked.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> Tags = new();

    /// <summary>
    /// Markup added to the gun on examine to display the upgrades.
    /// </summary>
    [DataField]
    public LocId ExamineText;
}
