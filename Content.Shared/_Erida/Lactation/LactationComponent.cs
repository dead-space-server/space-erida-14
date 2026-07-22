// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.Lactation;

[RegisterComponent, AutoGenerateComponentState, AutoGenerateComponentPause, NetworkedComponent]
public sealed partial class LactationComponent : Component
{
    [DataField, AutoNetworkedField] public bool IsMilkIncreased = false;
    [DataField, AutoNetworkedField] public float MilkIncreasedMultiplier = 1.25f;

    [DataField, AutoNetworkedField] public ProtoId<ReagentPrototype> ReagentId = "Milk";

    public string SolutionName = "lactation";

    [ViewVariables(VVAccess.ReadOnly)] public Entity<SolutionComponent>? Solution = null;

    [DataField, AutoNetworkedField] public FixedPoint2 QuantityPerUpdate = 25;

    [DataField, AutoNetworkedField] public FixedPoint2 QuantityPerUse = 5;

    public FixedPoint2 MaxQuantity = 50;

    [DataField, AutoNetworkedField] public float HungerUsage = 10f;

    [DataField, AutoNetworkedField] public TimeSpan GrowthDelay = TimeSpan.FromSeconds(1);

    [DataField, AutoPausedField] public TimeSpan CollectingTime = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField] public TimeSpan NextGrowth = TimeSpan.Zero;

    [DataField, AutoNetworkedField] public SoundSpecifier? DrinkSound;

    public string[] IncreasedMilkRaces = [
        "Demon"
    ];
}
