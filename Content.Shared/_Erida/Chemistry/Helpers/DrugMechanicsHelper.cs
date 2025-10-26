using Content.Shared.Chemistry.Components;

public static class DrugMechanicsHelper
{

    public static float CalculateAdjustedDrugEffect(float originalEffect, float tolerance)
    {

        return originalEffect * (1f - tolerance / 100f);
    }


    public static float ApplyAndUpdateTolerance(IEntityManager entityManager, EntityUid playerEntity, string drugId, float originalEffect, float toleranceIncrement)
    {
        var medTolComponent = entityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        medTolComponent.IncrementTolerance(drugId, toleranceIncrement);
        return CalculateAdjustedDrugEffect(originalEffect, medTolComponent.GetTolerance(drugId));
    }
}
