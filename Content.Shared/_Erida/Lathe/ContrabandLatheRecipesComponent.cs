using Content.Shared.Lathe.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Erida.Lathe
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ContrabandLatheRecipesComponent : Component
    {
        /// <summary>
        /// All of the dynamic recipe packs that the lathe is capable to get by cut contraband wire
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<ProtoId<LatheRecipePackPrototype>> ContrabandDynamicPacks = new();

        /// <summary>
        /// All of the static recipe packs that the lathe is capable to get by cut contraband wire
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<ProtoId<LatheRecipePackPrototype>> ContrabandStaticPacks = new();
    }

    [Serializable, NetSerializable]
    public enum ContrabandLatheWireKey : byte
    {
        StatusKey,
        TimeoutKey
    }
}
