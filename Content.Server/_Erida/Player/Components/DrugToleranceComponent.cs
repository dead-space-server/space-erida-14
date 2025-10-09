// Components/Player/DrugToleranceComponent.cs
using Content.Shared.Chemistry.Components;
using Robust.Shared.Serialization;

namespace Content.Server.Player.Components;

[RegisterComponent]
public sealed partial class DrugToleranceComponent : Component
{
    // Словарь для хранения уровня толерантности к каждому веществу
    public Dictionary<string, float> Tolerances { get; set; } = new();

    // Метод автоматического добавления препарата в словарь толерантности
    public void AddOrIncreaseTolerance(string drugName, float initialTolerance = 0.1f)
    {
        if (Tolerances.ContainsKey(drugName))
        {
            // Если препарат уже есть, увеличиваем имеющийся уровень толерантности
            Tolerances[drugName] += initialTolerance;
        }
        else
        {
            // Новый препарат добавляется с начальным уровнем толерантности
            Tolerances.Add(drugName, initialTolerance);
        }
    }

    // Метод увеличения уровня толерантности
    public void IncreaseTolerance(string drugName, float increaseAmount)
    {
        if (Tolerances.ContainsKey(drugName))
        {
            Tolerances[drugName] += increaseAmount;
        }
        else
        {
            Tolerances.Add(drugName, increaseAmount);
        }
    }

    // Получение текущего уровня толерантности
    public float GetTolerance(string drugName)
    {
        return Tolerances.TryGetValue(drugName, out float tolerance) ? tolerance : 0f;
    }
}
