using Content.Server._Erida.Objectives;

/// <summary>
/// Requirement to verify the presence of the target owner at the station
/// </summary>
[RegisterComponent, Access(typeof(NoOwnerRequirementSystem))]
public sealed partial class NoOwnerRequirementComponent : Component
{
    /// <summary>
    /// List of valid jobs for the target owner to satisfy the requirement
    /// </summary>
    [DataField(required: true)]
    public HashSet<string> Job = [];

    /// <summary>
    /// Chance to ignore the requirement check
    /// </summary>
    [DataField]
    public float IgnoreChance = 0.40f;
}
