using System.Linq;
using Content.Shared.Clothing;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Inventory;

/// <summary>
/// Handles prevention of items being unequipped and equipped from slots that are blocked by <see cref="SlotBlockComponent"/>.
/// </summary>
public sealed partial class SlotBlockSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlotBlockComponent, InventoryRelayedEvent<IsEquippingTargetAttemptEvent>>(OnEquipAttempt);
        SubscribeLocalEvent<SlotBlockComponent, InventoryRelayedEvent<IsUnequippingTargetAttemptEvent>>(OnUnequipAttempt);
        // Erida start
        SubscribeLocalEvent<SlotBlockComponent, GotUnequippedEvent>(UnequippedAttempt);
        SubscribeLocalEvent<SlotBlockComponent, ClothingGotEquippedEvent>(EquippedAttempt);
    }

    [Dependency] private InventorySystem _inventorySystem = default!;

    private void UpdateSlotsBlocking(EntityUid uid)
    {
        if (_inventorySystem.TryGetSlots(uid, out var slots))
        {
            HashSet<SlotFlags> blockList = new();
            HashSet<SlotFlags> hideList = new();

            foreach (var slot in slots)
            {
                if (_inventorySystem.TryGetSlotContainer(uid, slot.Name, out var slotContainer, out var slotDefinition)
                    && slotContainer.ContainedEntity != null
                    && TryComp<SlotBlockComponent>(slotContainer.ContainedEntity, out var comp))
                {
                    blockList.UnionWith(comp.BlockList);
                    hideList.UnionWith(comp.HideList);
                }
            }

            hideList.ExceptWith(blockList);

            foreach (var slot in slots)
            {
                if (blockList.Contains(slot.SlotFlags))
                    slot.StripBlocked = true;
                else
                {
                    slot.StripBlocked = false;

                    if (hideList.Contains(slot.SlotFlags))
                        slot.StripHiddenForce = true;
                    else
                        slot.StripHiddenForce = false;
                }
            }
        }
    }
    // Erida end

    private void OnEquipAttempt(Entity<SlotBlockComponent> ent, ref InventoryRelayedEvent<IsEquippingTargetAttemptEvent> args)
    {
        if (args.Args.Cancelled || !ent.Comp.BlockList.Contains(args.Args.SlotFlags)) // Erida edit
            return;

        args.Args.Reason = Loc.GetString("slot-block-component-blocked", ("item", ent));
        args.Args.Cancel();
    }

    private void OnUnequipAttempt(Entity<SlotBlockComponent> ent, ref InventoryRelayedEvent<IsUnequippingTargetAttemptEvent> args)
    {
        if (args.Args.Cancelled || !ent.Comp.BlockList.Contains(args.Args.SlotFlags)) // Erida edit
            return;

        args.Args.Reason = Loc.GetString("slot-block-component-blocked", ("item", ent));
        args.Args.Cancel();
    }

    // Erida start
    private void EquippedAttempt(Entity<SlotBlockComponent> ent, ref ClothingGotEquippedEvent args)
    {
        UpdateSlotsBlocking(args.Wearer);
    }

    private void UnequippedAttempt(Entity<SlotBlockComponent> ent, ref GotUnequippedEvent args)
    {
        UpdateSlotsBlocking(args.EquipTarget);
    }
    // Erida end
}
