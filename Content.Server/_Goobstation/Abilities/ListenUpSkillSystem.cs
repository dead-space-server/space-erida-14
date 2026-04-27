using Content.Shared._GoobStation.Resomi.Abilities.Hearing;

namespace Content.Server._GoobStation.Resomi.Abilities;

public sealed class ListenUpSkillSystem : SharedListenUpSkillSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ListenUpSkillComponent, ComponentInit>(OnComponentInit);
    }
    private void OnComponentInit(Entity<ListenUpSkillComponent> ent, ref ComponentInit args)
    {
        _actionsSystem.AddAction(ent.Owner, ref ent.Comp.SwitchListenUpActionEntity, ent.Comp.SwitchListenUpAction, ent.Owner);
    }
}
