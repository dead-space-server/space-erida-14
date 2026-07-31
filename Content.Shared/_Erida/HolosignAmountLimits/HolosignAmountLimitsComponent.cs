// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.HolosignAmountLimits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class HolosignAmountLimitsComponent : Component
{
    [DataField]
    public int MaxAmount = 6;

    [DataField, AutoNetworkedField]
    public int CurrentAmount = 0;

    /// <summary>
    /// The prototype to spawn on use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId SignProto = "HolosignWetFloor";

    /// <summary>
    /// Whether or not to use predictive spawning.
    /// At the moment this does not support entities with animated sprites, so set this to false in that case.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool PredictedSpawn;

    public HashSet<EntityUid> SpawnedSigns = new();
}
