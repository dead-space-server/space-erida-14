namespace Content.Shared.Erida.MaterialEnergy;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class MaterialEnergyComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string>? MaterialWhiteList;
}
