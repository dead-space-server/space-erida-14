using Robust.Shared.GameStates;

namespace Content.Shared._Erida.Traits.Masochist
{
    /// <summary>
    /// Increases arousal when owner taking damage.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class MasochistComponent : Component
    {
        /// <summary>
        /// List of damage thresholds that will cause arousal
        /// DamageType : Threshold
        /// </summary>
        [DataField]
        public Dictionary<string, float> DamageThreshold = new()
        {
            {"Blunt", 25f},
            {"Thermal", 20f},
            {"Piercing", 20f},
            {"Shock", 25f}
        };

        /// <summary>
        /// Multiplier for arousal increase per unit of damage taken.
        /// </summary>
        [DataField]
        public float ArousalPerDamageModifier = 1f;

        /// <summary>
        /// The total degree of damage after which arousal doesnt increase.
        /// </summary>
        [DataField]
        public float TotalDamageLimit = 40f;
    }
}
