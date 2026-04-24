using Content.Server.Botany.Components;
using Content.Server.Botany.Systems;
using Content.Server.Botany;
using Content.Shared._Erida.Botany.SeedDna;
using Content.Shared._Erida.Botany.SeedDna.Components;
using Content.Shared._Erida.Botany.SeedDna.Systems;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using System.Linq;

namespace Content.Server._Erida.Botany.Systems;

[UsedImplicitly]
public sealed class SeedDnaConsoleSystem : SharedSeedDnaConsoleSystem
{
    [Dependency] private readonly BotanySystem _botany = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeedDnaConsoleComponent, WriteToTargetSeedDataMessage>(OnWriteToTargetSeedDataMessage);

        SubscribeLocalEvent<SeedDnaConsoleComponent, ComponentStartup>(OnUpdateUserInterface);
        SubscribeLocalEvent<SeedDnaConsoleComponent, EntInsertedIntoContainerMessage>(OnUpdateUserInterface);
        SubscribeLocalEvent<SeedDnaConsoleComponent, EntRemovedFromContainerMessage>(OnUpdateUserInterface);
    }

    private void OnUpdateUserInterface(EntityUid uid, SeedDnaConsoleComponent component, EntityEventArgs args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnWriteToTargetSeedDataMessage(EntityUid uid, SeedDnaConsoleComponent component, WriteToTargetSeedDataMessage args)
    {
        if (args.Target == TargetSeedData.Seed && component.SeedSlot.Item is { Valid: true } seedItem)
            RewriteSeedData(seedItem, args.SeedDataDto);
        else if (args.Target == TargetSeedData.DnaDisk && component.DnaDiskSlot.Item is { Valid: true } dnaDiskItem)
            RewriteDnaDiskData(dnaDiskItem, args.SeedDataDto);

        UpdateUserInterface(uid, component);
    }

    private void UpdateUserInterface(EntityUid uid, SeedDnaConsoleComponent component)
    {
        if (!component.Initialized)
            return;

        var (seedPresent, seedName, seedData) = ProcessSeedSlot(component);
        var (dnaDiskPresent, dnaDiskName, dnaDiskData) = ProcessDiskSlot(component);

        var newState = new SeedDnaConsoleBoundUserInterfaceState(
            seedPresent,
            seedName,
            seedData,
            dnaDiskPresent,
            dnaDiskName,
            dnaDiskData
        );
        _userInterface.SetUiState(uid, SeedDnaConsoleUiKey.Key, newState);
    }

    private (bool, string, SeedDataDto?) ProcessSeedSlot(SeedDnaConsoleComponent component)
    {
        return component.SeedSlot.Item is not { Valid: true } seedItem
            ? (false, string.Empty, null)
            : (true, MetaData(seedItem).EntityName, ExtractSeedData(seedItem));
    }

    private void RewriteSeedData(EntityUid seed, SeedDataDto seedDataDto)
    {
        var seedComponent = Comp<SeedComponent>(seed);

        if (!_botany.TryGetSeed(seedComponent, out var originalSeedData))
            return;

        var seedData = originalSeedData.Clone();
        seedComponent.Seed = seedData;

        if (seedDataDto.ConsumeGasses != null) seedData.ConsumeGasses = new(seedDataDto.ConsumeGasses);
        if (seedDataDto.ExudeGasses != null) seedData.ExudeGasses = new(seedDataDto.ExudeGasses);
        if (seedDataDto.NutrientConsumption != null) seedData.NutrientConsumption = seedDataDto.NutrientConsumption.Value;
        if (seedDataDto.WaterConsumption != null) seedData.WaterConsumption = seedDataDto.WaterConsumption.Value;
        if (seedDataDto.IdealHeat != null) seedData.IdealHeat = seedDataDto.IdealHeat.Value;
        if (seedDataDto.HeatTolerance != null) seedData.HeatTolerance = seedDataDto.HeatTolerance.Value;
        if (seedDataDto.IdealLight != null) seedData.IdealLight = seedDataDto.IdealLight.Value;
        if (seedDataDto.LightTolerance != null) seedData.LightTolerance = seedDataDto.LightTolerance.Value;
        if (seedDataDto.ToxinsTolerance != null) seedData.ToxinsTolerance = seedDataDto.ToxinsTolerance.Value;
        if (seedDataDto.LowPressureTolerance != null) seedData.LowPressureTolerance = seedDataDto.LowPressureTolerance.Value;
        if (seedDataDto.HighPressureTolerance != null) seedData.HighPressureTolerance = seedDataDto.HighPressureTolerance.Value;
        if (seedDataDto.PestTolerance != null) seedData.PestTolerance = seedDataDto.PestTolerance.Value;
        if (seedDataDto.WeedTolerance != null) seedData.WeedTolerance = seedDataDto.WeedTolerance.Value;
        if (seedDataDto.WeedHighLevelThreshold != null) seedData.WeedHighLevelThreshold = seedDataDto.WeedHighLevelThreshold.Value;
        if (seedDataDto.Endurance != null) seedData.Endurance = seedDataDto.Endurance.Value;
        if (seedDataDto.Yield != null) seedData.Yield = seedDataDto.Yield.Value;
        if (seedDataDto.Lifespan != null) seedData.Lifespan = seedDataDto.Lifespan.Value;
        if (seedDataDto.Maturation != null) seedData.Maturation = seedDataDto.Maturation.Value;
        if (seedDataDto.Production != null) seedData.Production = seedDataDto.Production.Value;
        if (seedDataDto.HarvestRepeat != null) seedData.HarvestRepeat = (HarvestType) (byte) seedDataDto.HarvestRepeat.Value;
        if (seedDataDto.Potency != null) seedData.Potency = seedDataDto.Potency.Value;
        if (seedDataDto.Seedless != null) seedData.Seedless = seedDataDto.Seedless.Value;
        if (seedDataDto.Viable != null) seedData.Viable = seedDataDto.Viable.Value;
        if (seedDataDto.Ligneous != null) seedData.Ligneous = seedDataDto.Ligneous.Value;
        if (seedDataDto.CanScream != null) seedData.CanScream = seedDataDto.CanScream.Value;
        if (seedDataDto.TurnIntoKudzu != null) seedData.TurnIntoKudzu = seedDataDto.TurnIntoKudzu.Value;

        if (seedDataDto.Chemicals == null)
            return;

        seedData.Chemicals.Clear();

        const float maxProduceVolume = 100f;
        var currentVolume = FixedPoint2.Zero;

        foreach (var (key, value) in seedDataDto.Chemicals)
        {
            var seedChemQuantity = new SeedChemQuantity
            {
                Min = value.Min,
                Max = value.Max,
                PotencyDivisor = value.PotencyDivisor,
                Inherent = value.Inherent,
            };

            var chemVolume = value.Max;

            if ((currentVolume + chemVolume).Float() > maxProduceVolume)
            {
                var volumeNeeded = currentVolume + chemVolume - FixedPoint2.New(maxProduceVolume);
                var chemicalKeys = seedData.Chemicals.Keys.ToList();
                var keyIndex = 0;

                while (volumeNeeded > FixedPoint2.Zero && keyIndex < chemicalKeys.Count)
                {
                    var oldKey = chemicalKeys[keyIndex];
                    var oldChem = seedData.Chemicals[oldKey];

                    currentVolume -= oldChem.Max;
                    volumeNeeded -= oldChem.Max;
                    seedData.Chemicals.Remove(oldKey);
                    keyIndex++;
                }
            }

            seedData.Chemicals[key] = seedChemQuantity;
            currentVolume += chemVolume;
        }
    }

    private void RewriteDnaDiskData(EntityUid dnaDisk, SeedDataDto dnaDiskDataDto)
    {
        Comp<DnaDiskComponent>(dnaDisk).SeedData = dnaDiskDataDto;
    }

    private SeedDataDto? ExtractSeedData(EntityUid seed)
    {
        if (!TryComp<SeedComponent>(seed, out var seedComp) ||
            !_botany.TryGetSeed(seedComp, out var seedData))
        {
            return null;
        }

        var seedDataDto = new SeedDataDto
        {
            ConsumeGasses = new(seedData.ConsumeGasses),
            ExudeGasses = new(seedData.ExudeGasses),
            NutrientConsumption = seedData.NutrientConsumption,
            WaterConsumption = seedData.WaterConsumption,
            IdealHeat = seedData.IdealHeat,
            HeatTolerance = seedData.HeatTolerance,
            IdealLight = seedData.IdealLight,
            LightTolerance = seedData.LightTolerance,
            ToxinsTolerance = seedData.ToxinsTolerance,
            LowPressureTolerance = seedData.LowPressureTolerance,
            HighPressureTolerance = seedData.HighPressureTolerance,
            PestTolerance = seedData.PestTolerance,
            WeedTolerance = seedData.WeedTolerance,
            WeedHighLevelThreshold = seedData.WeedHighLevelThreshold,
            Endurance = seedData.Endurance,
            Yield = seedData.Yield,
            Lifespan = seedData.Lifespan,
            Maturation = seedData.Maturation,
            Production = seedData.Production,
            HarvestRepeat = (SharedHarvestTypeDto) (byte) seedData.HarvestRepeat,
            Potency = seedData.Potency,
            Seedless = seedData.Seedless,
            Viable = seedData.Viable,
            Ligneous = seedData.Ligneous,
            CanScream = seedData.CanScream,
            TurnIntoKudzu = seedData.TurnIntoKudzu,
            Chemicals = new Dictionary<string, SeedChemQuantityDto>(),
        };

        foreach (var (key, value) in seedData.Chemicals)
        {
            seedDataDto.Chemicals[key] = new SeedChemQuantityDto
            {
                Min = value.Min,
                Max = value.Max,
                PotencyDivisor = value.PotencyDivisor,
                Inherent = value.Inherent,
            };
        }

        return seedDataDto;
    }

    private (bool, string, SeedDataDto?) ProcessDiskSlot(SeedDnaConsoleComponent component)
    {
        return component.DnaDiskSlot.Item is not { Valid: true } diskItem
            ? (false, string.Empty, null)
            : (true, MetaData(diskItem).EntityName, ExtractDiskData(diskItem));
    }

    private SeedDataDto? ExtractDiskData(EntityUid dnaDisk)
    {
        return Comp<DnaDiskComponent>(dnaDisk).SeedData;
    }
}
