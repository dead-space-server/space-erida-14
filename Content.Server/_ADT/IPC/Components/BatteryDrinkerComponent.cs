namespace Content.Server._ADT.Power;

[RegisterComponent]
public sealed partial class BatteryDrinkerComponent : Component
{
    [DataField]
    public bool DrinkAll;

    [DataField]
    public float DrinkSpeed = 1.5f;

    [DataField]
    public float DrinkMultiplier = 5f;

    [DataField]
    public float DrinkAllMultiplier = 2.5f;
}
