using Content.Shared._Erida.Chemistry.Components;

namespace Content.Shared._Erida.Chemistry.Helpers;

public static class DrugMechanicsHelper
{

    public static float CalculateAdjustedDrugEffect(float originalEffect, float tolerance)
    {
        return originalEffect * (1f - tolerance / 100f);
    }
}
