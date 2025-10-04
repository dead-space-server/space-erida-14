// File: DrugToleranceComponent.cs
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameObjects;

namespace Content.Shared.Drugs.Components
{
    [RegisterComponent]
    public sealed partial class DrugToleranceComponent : Component
    {
        // Словарь для хранения уровня толерантности к различным веществам
        public Dictionary<string, float> Tolerances { get; set; } = new Dictionary<string, float>();

        // Методы для управления уровнем толерантности
        public void AddTolerance(string reagentID, float amount)
        {
            if (Tolerances.ContainsKey(reagentID))
            {
                Tolerances[reagentID] += amount;
            }
            else
            {
                Tolerances[reagentID] = amount;
            }
        }

        public void ResetTolerance(string reagentID)
        {
            Tolerances.Remove(reagentID);
        }

        public float GetTolerance(string reagentID)
        {
            return Tolerances.TryGetValue(reagentID, out var value) ? value : 0f;
        }
    }
}
