using Content.Server.Hands.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Chat;

/// <summary>
/// Transfered from SuicideSystem, because OnEnvironmentalSuicide always should be handled first
/// </summary>

public sealed partial class SuicideEnviromentalSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private EntityQuery<ItemComponent> _itemQuery = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, SuicideEvent>(OnEnvironmentalSuicide, before: [typeof(SuicideSystem)]);
    }

    /// <summary>
    /// Raise event to attempt to use held item, or surrounding entities to attempt to commit suicide
    /// </summary>
    private void OnEnvironmentalSuicide(Entity<MobStateComponent> victim, ref SuicideEvent args)
    {
        if (args.Handled || _mobState.IsCritical(victim))
            return;

        var suicideByEnvironmentEvent = new SuicideByEnvironmentEvent(victim);

        // Try to suicide by raising an event on the held item
        if (_hands.TryGetActiveItem(victim.Owner, out var item))
        {
            RaiseLocalEvent(item.Value, suicideByEnvironmentEvent);
            if (suicideByEnvironmentEvent.Handled)
            {
                args.Handled = suicideByEnvironmentEvent.Handled;
                return;
            }
        }

        // Try to suicide by nearby entities, like Microwaves or Crematoriums, by raising an event on it
        // Returns upon being handled by any entity
        foreach (var entity in _entityLookupSystem.GetEntitiesInRange(victim, 1, LookupFlags.Approximate | LookupFlags.Static))
        {
            // Skip any nearby items that can be picked up, we already checked the active held item above
            if (_itemQuery.HasComponent(entity))
                continue;

            RaiseLocalEvent(entity, suicideByEnvironmentEvent);
            if (!suicideByEnvironmentEvent.Handled)
                continue;

            args.Handled = suicideByEnvironmentEvent.Handled;
            return;
        }
    }
}
