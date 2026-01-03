using Content.Shared._Erida.DirectionalEmote;
using Content.Shared.Verbs;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client._Erida.DirectionalEmote;

public sealed partial class DirectionalEmoteSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DirectionalEmoteTargetComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    public void ShowMessage(NetEntity source, NetEntity target, string text)
    {
        if (text == "" || text.Length > 10000)
            return;

        RaiseNetworkEvent(new SendDirectionalEmoteEvent(source, target, text));
    }

    private void OnGetVerbs(EntityUid uid, DirectionalEmoteTargetComponent component, GetVerbsEvent<Verb> args)
    {
        if (args.Target == args.User ||
            !HasComp<DirectionalEmoteTargetComponent>(args.User))
            return;

        args.Verbs.Add(new Verb
        {
            Act = () => OpenWindow(GetNetEntity(args.User), GetNetEntity(args.Target)),
            Priority = 15,
            Text = Loc.GetString("directional-emote-verb-name"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/emotes.svg.192dpi.png")),
            ClientExclusive = true,
        });
    }

    private void OpenWindow(NetEntity source, NetEntity target)
    {
        _userInterfaceManager.GetUIController<DirectionalEmoteUIController>().ToggleWindow(source, target);
    }
}
