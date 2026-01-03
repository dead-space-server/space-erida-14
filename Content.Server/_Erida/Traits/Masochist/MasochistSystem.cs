using Content.Server._Erida.Arousal;
using Content.Shared._Erida.Traits.Masochist;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;

namespace Content.Server._Erida.Traits.Masochist;

public sealed class MasochistSystem : EntitySystem
{
    [Dependency] private readonly ArousalSystem _arousalSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MasochistComponent, DamageChangedEvent>(OnDamageChanged);
    }

    public void OnDamageChanged(Entity<MasochistComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        if (!HasComp<MobStateComponent>(ent.Owner))
            return;

        var damageable = CompOrNull<DamageableComponent>(ent.Owner);
        if (damageable == null || damageable.TotalDamage >= ent.Comp.TotalDamageLimit)
            return;

        // TODO add debaff after total damage limit is reached
        foreach (var damage in args.DamageDelta.DamageDict)
        {
            if (!ent.Comp.DamageThreshold.ContainsKey(damage.Key))
                continue;

            if (ent.Comp.DamageThreshold[damage.Key] <= damageable.Damage[damage.Key])
                continue;

            _arousalSystem.IncreaseArousal(ent.Owner, damage.Value.Float() * ent.Comp.ArousalPerDamageModifier);
        }
    }
}
