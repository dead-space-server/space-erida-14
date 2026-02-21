using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class WaterVaporDecompositionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHyperNoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHyperNoblium >= 5.0f && mixture.Temperature > 20f)
            return ReactionResult.NoReaction;

        if (mixture.GetMoles(Gas.BZ) < 0.01f)
            return ReactionResult.NoReaction;

        float waterVapor = mixture.GetMoles(Gas.WaterVapor);
        if (waterVapor < 0.01f)
            return ReactionResult.NoReaction;

        float amountToReact = waterVapor * 0.1f;
        if (amountToReact < 0.01f)
            amountToReact = waterVapor;

        mixture.AdjustMoles(Gas.WaterVapor, -amountToReact);

        mixture.AdjustMoles(Gas.Hydrogen, amountToReact * 0.66f);
        mixture.AdjustMoles(Gas.Oxygen, amountToReact * 0.33f);

        return ReactionResult.Reacting;
    }
}