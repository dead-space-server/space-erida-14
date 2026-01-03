using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // hissss
        message = RegexLowerS.Replace(message, "sss");
        // hiSSS
        message = RegexUpperS.Replace(message, "SSS");
        // ekssit
        message = RegexInternalX.Replace(message, "$1kss");
        // ecks
        message = RegexLowerEndX.Replace(message, "ecks$1");
        // eckS
        message = RegexUpperEndX.Replace(message, "ECKS$1");

        // erida-loc-start
        // c > ccc / С > ССС
        message = Regex.Replace(message, "с+", _random.Pick(new List<string>() { "сс", "ссс" }));
        message = Regex.Replace(message, "С+", _random.Pick(new List<string>() { "СС", "ССС" }));

        // ш > шшш / Ш > ШШШ
        message = Regex.Replace(message, "ш+", _random.Pick(new List<string>() { "шш", "шшш" }));
        message = Regex.Replace(message, "Ш+", _random.Pick(new List<string>() { "ШШ", "ШШШ" }));

        // ч > щщщ / Ч > ЩЩЩ
        message = Regex.Replace(message, "ч+", _random.Pick(new List<string>() { "щщ", "щщщ" }));
        message = Regex.Replace(message, "Ч+", _random.Pick(new List<string>() { "ЩЩ", "ЩЩЩ" }));
        // erida-loc-end

        args.Message = message;
    }
}
