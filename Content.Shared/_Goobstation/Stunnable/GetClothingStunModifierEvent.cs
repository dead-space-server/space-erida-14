using Robust.Shared.GameObjects;

namespace Content.Shared._Goobstation.Stunnable;

public sealed class GetClothingStunModifierEvent : EntityEventArgs
{
    public GetClothingStunModifierEvent(EntityUid target)
    {
        Target = target;
    }

    public EntityUid Target;
    public float Modifier = 1f;
}
