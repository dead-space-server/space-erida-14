using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared._Erida.Strip
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class SlotBlockComponent : Component
    {
        /// <summary>
        /// Slots that this entity should block and hide.
        /// </summary>
        [DataField("blockList"), AutoNetworkedField]
        public HashSet<SlotFlags> BlockList = new();
        /// <summary>
        /// Slots that this entity should only hide.
        /// </summary>
        [DataField("hideList"), AutoNetworkedField]
        public HashSet<SlotFlags> HideList = new();
    }
}
