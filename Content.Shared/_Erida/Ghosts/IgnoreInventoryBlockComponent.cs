using Robust.Shared.GameStates;

namespace Content.Shared._Erida.Ghosts
{
    [RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
    public sealed partial class IgnoreInventoryBlockComponent : Component
    {
        [DataField("ignoreBlock"), AutoNetworkedField]
        public bool IgnoreBlock = true;

        [DataField("showAllItems"), AutoNetworkedField]
        public bool ShowAllItems = true;
    }
}
