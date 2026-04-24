using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

// ReSharper disable once CheckNamespace
namespace Content.Shared._Erida.Botany.SeedDna;

[DataDefinition]
[Serializable, NetSerializable]
public partial struct SeedChemQuantityDto
{
    [DataField("Min")] public FixedPoint2 Min;
    [DataField("Max")] public FixedPoint2 Max;
    [DataField("PotencyDivisor")] public float PotencyDivisor;
    [DataField("Inherent")] public bool Inherent = true;

    public bool Equals(SeedChemQuantityDto other)
    {
        return Min == other.Min
               && Max == other.Max
               && Math.Abs(PotencyDivisor - other.PotencyDivisor) < 0.0001f
               && Inherent == other.Inherent;
    }

    public override bool Equals(object? obj)
    {
        return obj is SeedChemQuantityDto other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max, PotencyDivisor, Inherent);
    }
}
