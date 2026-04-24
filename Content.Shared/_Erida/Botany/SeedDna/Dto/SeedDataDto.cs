using Content.Shared.Atmos;
using Robust.Shared.Serialization;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Content.Shared._Erida.Botany.SeedDna;

[Serializable, NetSerializable]
public sealed class SeedDataDto
{
    public Dictionary<string, SeedChemQuantityDto>? Chemicals;
    public Dictionary<Gas, float>? ConsumeGasses;
    public Dictionary<Gas, float>? ExudeGasses;
    public float? NutrientConsumption;
    public float? WaterConsumption;
    public float? IdealHeat;
    public float? HeatTolerance;
    public float? IdealLight;
    public float? LightTolerance;
    public float? ToxinsTolerance;
    public float? LowPressureTolerance;
    public float? HighPressureTolerance;
    public float? PestTolerance;
    public float? WeedTolerance;
    public float? WeedHighLevelThreshold;
    public float? Endurance;
    public int? Yield;
    public float? Lifespan;
    public float? Maturation;
    public float? Production;
    public SharedHarvestTypeDto? HarvestRepeat;
    public float? Potency;
    public bool? Seedless;
    public bool? Viable;
    public bool? Ligneous;
    public bool? CanScream;
    public bool? TurnIntoKudzu;
}

[Serializable, NetSerializable]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum SharedHarvestTypeDto : byte
{
    NoRepeat,
    Repeat,
    SelfHarvest,
}
