using System.Text.RegularExpressions;
using Content.Server._Erida.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._Erida.Speech.EntitySystems;

public sealed class RoarAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private static readonly Regex RegexR = new("Р");
    private static readonly Regex Regexr = new("р");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoarAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, RoarAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // р > ррр / Р > РРР
        message = Regexr.Replace(message, _random.Pick(new List<string>() { "рр", "ррр" }));
        message = RegexR.Replace(message, _random.Pick(new List<string>() { "РР", "РРР" }));

        args.Message = message;
    }
}
