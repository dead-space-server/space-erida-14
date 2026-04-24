using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Shared._Goobstation.Inventory;

public sealed partial class GoobInventorySystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    private EntityQuery<ContainerManagerComponent> _managerQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeRelays();

        _metaQuery = GetEntityQuery<MetaDataComponent>();
        _managerQuery = GetEntityQuery<ContainerManagerComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    private bool IsContainerValid(EntityUid uid, HashSet<EntityUid> toRemove)
    {
        if (toRemove.Contains(uid))
            return false;

        var parent = _xformQuery.Comp(uid).ParentUid;
        var child = uid;

        while (parent.IsValid())
        {
            if ((_metaQuery.GetComponent(child).Flags & MetaDataFlags.InContainer) == MetaDataFlags.InContainer &&
                _managerQuery.TryGetComponent(parent, out var conManager) &&
                _container.TryGetContainingContainer(parent, child, out var parentContainer, conManager) &&
                toRemove.Contains(parentContainer.Owner))
                return false;

            var parentXform = _xformQuery.GetComponent(parent);
            child = parent;
            parent = parentXform.ParentUid;
        }

        return true;
    }
}
