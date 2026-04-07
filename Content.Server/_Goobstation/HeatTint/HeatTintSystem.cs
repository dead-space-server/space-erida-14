using Content.Shared._Goobstation.HeatTint;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;

namespace Content.Server._Goobstation.HeatTint;

public sealed class HeatTintSystem : SharedHeatTintSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatTintComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HeatTintComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<HeatTintComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
    }

    private void OnMapInit(Entity<HeatTintComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<TemperatureComponent>(ent, out var temp))
            _appearance.SetData(ent, HeatTintVisuals.Temperature, temp.CurrentTemperature);
    }

    private void OnTemperatureChange(Entity<HeatTintComponent> ent, ref OnTemperatureChangeEvent args)
    {
        _appearance.SetData(ent, HeatTintVisuals.Temperature, args.CurrentTemperature);
    }

    private void OnSolutionChanged(Entity<HeatTintComponent> ent, ref SolutionContainerChangedEvent args)
    {
        _appearance.SetData(ent, HeatTintVisuals.Temperature, args.Solution.Temperature);
    }
}
