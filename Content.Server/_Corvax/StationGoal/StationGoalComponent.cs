using Robust.Shared.Prototypes;

namespace Content.Server._Corvax.StationGoal
{
    /// <summary>
    ///     if attached to a station prototype, will send the station a random goal from the list
    /// </summary>
    [RegisterComponent]
    public sealed partial class StationGoalComponent : Component
    {
        [DataField]
        public List<ProtoId<StationGoalPrototype>> BlackListedGoals = new();

        [DataField]
        public List<ProtoId<StationGoalPrototype>> WhiteListedGoals = new();

        [DataField]
        public List<ProtoId<StationGoalPrototype>> SpecialGoals = new();
    }
}
