using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._Erida.CkeyCode;

public sealed partial class CkeyCodeSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CkeyCodeComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<CkeyCodeComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CkeyCodeComponent, GotEquippedEvent>(OnEquip);
    }

    private void OnComponentShutdown(EntityUid uid, CkeyCodeComponent component, ComponentShutdown args)
    {
        QueueDel(uid); // because we cannot allow unregistered users to use it
    }

    private void OnComponentInit(EntityUid uid, CkeyCodeComponent component, ComponentInit args)
    {
        component.LastSecond = (int)component.ExplosionTime;
    }

    private void OnEquip(EntityUid uid, CkeyCodeComponent component, GotEquippedEvent args)
    {
        if (TryComp<ActorComponent>(args.Equipee, out var actor) && component.Ckeys.Contains(actor.PlayerSession.Name))
        {
            _chatSystem.TrySendInGameICMessage(uid, Loc.GetString("identification-success"), InGameICChatType.Speak, true);
            _audioSystem.PlayPvs(component.AcceptedSound, uid);
            return;
        }

        _chatSystem.TrySendInGameICMessage(uid, Loc.GetString("identification-failed"), InGameICChatType.Speak, true);
        _audioSystem.PlayPvs(component.DeniedSound, uid);

        EnsureComp<UnremoveableComponent>(uid);

        component.Wearer = args.Equipee;
        component.IsArmed = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CkeyCodeComponent>();

        while (query.MoveNext(out var uid, out var ckeyCode))
        {
            if (!ckeyCode.IsArmed)
                continue;

            ckeyCode.ExplosionTime -= frameTime;

            if ((int)ckeyCode.ExplosionTime != ckeyCode.LastSecond)
            {
                ckeyCode.LastSecond = (int)ckeyCode.ExplosionTime;
                _chatSystem.TrySendInGameICMessage(uid, Loc.GetString($"{ckeyCode.LastSecond}. . ."), InGameICChatType.Speak, true);
            }

            if (ckeyCode.ExplosionTime <= 0)
            {
                _explosionSystem.QueueExplosion(uid, "Default", 10, 4, 1, 0);
                _bodySystem.GibBody(ckeyCode.Wearer);
            }
        }
    }
}
