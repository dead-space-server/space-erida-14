namespace Content.Shared._Erida.Leash.Components;

[RegisterComponent]
public sealed partial class LeashHolderComponent : Component
{
    public readonly HashSet<EntityUid> Leashes = new();
}
