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

/// <summary>
/// <para> Adding lactation to entity. </para>
/// To interact with the target, the user must have <see cref="InteractionWhitelistComponent"/>.
/// </summary>
[RegisterComponent, AutoGenerateComponentState, AutoGenerateComponentPause, NetworkedComponent]
public sealed partial class LactationComponent : Component
{
    /// <summary>
    /// If enabled, it multiplies the amount of milk per minute by <see cref="MilkIncreasedMultiplier"/>
    /// <para> Automatically changes with comp init </para>
    /// </summary>
    [DataField, AutoNetworkedField] public bool IsMilkIncreased = false;

    public string[] IncreasedMilkRaces = [
        "Demon"
    ];

    [DataField, AutoNetworkedField] public float MilkIncreasedMultiplier = 1.25f;

    [DataField, AutoNetworkedField] public ProtoId<ReagentPrototype> ReagentId = "Milk";

    [DataField, AutoNetworkedField] public FixedPoint2 QuantityPerUpdate = 10;

    [DataField, AutoNetworkedField] public FixedPoint2 QuantityPerUse = 5;

    [DataField, AutoNetworkedField] public float HungerUsage = 10f;

    [DataField, AutoNetworkedField] public TimeSpan GrowthDelay = TimeSpan.FromMinutes(2);

    [DataField, AutoPausedField] public TimeSpan CollectingTime = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField] public TimeSpan NextGrowth = TimeSpan.Zero;

    [DataField, AutoNetworkedField] public SoundSpecifier? DrinkSound;

    [ViewVariables(VVAccess.ReadOnly)] public Entity<SolutionComponent>? Solution = null;

    public FixedPoint2 MaxQuantity = 60;

    public string SolutionName = "lactation";
}
