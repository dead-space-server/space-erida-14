using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Addictions;

public abstract partial class SharedAddictionSystem : EntitySystem
{
    [Dependency] private StatusEffectNew.StatusEffectsSystem _statusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "Addicted";

    protected abstract void UpdateTime(EntityUid uid);

    public virtual void TryApplyAddiction(EntityUid uid, float addictionTime, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        UpdateTime(uid);

        if (!_statusEffects.HasStatusEffect(uid, StatusEffectKey.Id))
        {
            _statusEffects.TryAddStatusEffect(uid, StatusEffectKey.Id, out var _, TimeSpan.FromSeconds(addictionTime));
        }
        else
        {
            _statusEffects.TryAddTime(uid, StatusEffectKey.Id, TimeSpan.FromSeconds(addictionTime));
        }
    }

    public virtual void TrySuppressAddiction(EntityUid uid, float duration)
    {
        if (!TryComp<AddictedComponent>(uid, out var comp))
            return;

        var ent = new Entity<AddictedComponent>(uid, comp);
        UpdateAddictionSuppression(ent, duration);
    }

    protected virtual void UpdateAddictionSuppression(Entity<AddictedComponent> ent, float duration)
    {
    }
}
