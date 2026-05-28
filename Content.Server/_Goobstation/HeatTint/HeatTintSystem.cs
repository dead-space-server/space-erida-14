using Content.Shared._Goobstation.HeatTint;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;

namespace Content.Server._Goobstation.HeatTint;

public sealed partial class HeatTintSystem : SharedHeatTintSystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatTintComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HeatTintComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<HeatTintComponent, SolutionChangedEvent>(OnSolutionChanged);
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

    private void OnSolutionChanged(Entity<HeatTintComponent> ent, ref SolutionChangedEvent args)
    {
        _appearance.SetData(ent, HeatTintVisuals.Temperature, args.Solution.Comp.Solution.Temperature);
    }
}
