namespace Content.Server.Ghost.Roles.Components
{
    /// <summary>
    ///     Allows a ghost to take over the Owner entity.
    /// </summary>
    [RegisterComponent]
    [Access(typeof(GhostRoleSystem))]
    public sealed partial class GhostTakeoverAvailableComponent : Component
    {
        [DataField, Access(Other = AccessPermissions.ReadWriteExecute)]
        public bool IgnoreMindCheck; // Goobstation
    }
}
