using Robust.Shared.GameStates;
using Content.Shared.Roles.Components;
using Content.Shared._Erida.Nightmare.Components;

namespace Content.Shared._Erida.Roles.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedNightmareSystem))]
public sealed partial class NightmareRoleComponent : BaseMindRoleComponent
{
    [DataField]
    public bool PolymorphState = false;
}
