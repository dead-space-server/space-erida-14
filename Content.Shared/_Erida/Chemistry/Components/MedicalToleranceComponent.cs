using System.Collections.Generic;

namespace Content.Shared.Chemistry.Components
{

    [RegisterComponent]
    public sealed partial class MedicalToleranceComponent : Component
    {

        [ViewVariables]
        public Dictionary<string, float> Tolerances { get; set; } = new Dictionary<string, float>();


        public void SetTolerance(string drugId, float tolerance)
        {
            Tolerances[drugId] = tolerance;
        }


        public void IncrementTolerance(string drugId, float increment)
        {
            if (!Tolerances.ContainsKey(drugId))
            {
                Tolerances.Add(drugId, 0f);
            }
            Tolerances[drugId] += increment;
        }


        public float GetTolerance(string drugId)
        {
            return Tolerances.TryGetValue(drugId, out var tolerance) ? tolerance : 0f;
        }
    }
}
