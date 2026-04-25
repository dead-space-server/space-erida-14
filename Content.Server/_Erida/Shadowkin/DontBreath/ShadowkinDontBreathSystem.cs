using Content.Server.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.DontBreath;

public sealed partial class DontBreathSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DontBreathComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DontBreathComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<DontBreathComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    private void OnInit(Entity<DontBreathComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<RespiratorComponent>(ent.Owner, out RespiratorComponent? respiratorComponent))
        {
            RemComp<DontBreathComponent>(ent.Owner);

            return;
        }

        ent.Comp.RespiratorData.Damage = respiratorComponent.Damage;
        ent.Comp.RespiratorData.DamageRecovery = respiratorComponent.DamageRecovery;
        ent.Comp.RespiratorData.GaspEmote = respiratorComponent.GaspEmote;

        respiratorComponent.GaspEmote = null;
        respiratorComponent.Damage = new DamageSpecifier();
        respiratorComponent.DamageRecovery = new DamageSpecifier();
    }

    private void OnShutdown(Entity<DontBreathComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<RespiratorComponent>(ent.Owner, out RespiratorComponent? respiratorComponent))
            return;

        respiratorComponent.Damage = ent.Comp.RespiratorData.DamageRecovery;
        respiratorComponent.DamageRecovery = ent.Comp.RespiratorData.DamageRecovery;
        respiratorComponent.GaspEmote = ent.Comp.RespiratorData.GaspEmote;
    }

    private void OnBeforeDamageChanged(Entity<DontBreathComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (!args.Damage.DamageDict.TryGetValue("Asphyxiation", out var damage))
            return;

        args.Damage.DamageDict[ent.Comp.AsphyxiationPrototype] = 0;
    }
}
