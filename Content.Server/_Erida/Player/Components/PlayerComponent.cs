// Components/Player/PlayerComponent.cs
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server.Player.Components;

[RegisterComponent]
public sealed partial class PlayerComponent : Component
{
    // Общий идентификатор игрока (может использоваться для логирования или статистики)
    public Guid Id { get; set; }

    // Специальные маркеры здоровья или состояния тела, важные для химических реакций
    public float HealthModifier { get; set; } = 1.0f; // Модификатор здоровья
    public float MetabolismFactor { get; set; } = 1.0f; // Скорость метаболизма
}
