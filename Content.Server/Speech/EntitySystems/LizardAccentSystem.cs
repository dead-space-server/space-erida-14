using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random; // Corvax-Localization

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    // Erida start
    private static readonly Regex Regex1 = new("с+");
    private static readonly Regex Regex2 = new("С+");
    private static readonly Regex Regex3 = new("з+");
    private static readonly Regex Regex4 = new("З+");
    private static readonly Regex Regex5 = new("ш+");
    private static readonly Regex Regex6 = new("Ш+");
    private static readonly Regex Regex7 = new("ч+");
    private static readonly Regex Regex8 = new("Ч+");
    // Erida end

    [Dependency] private readonly IRobustRandom _random = default!; // Corvax-Localization
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
        // Corvax-Localization-Start
        // c => ссс
        message = Regex1.Replace(
            message,
            _random.Pick(new List<string>() { "сс", "ссс" })
        );
        // С => CCC
        message = Regex2.Replace(
            message,
            _random.Pick(new List<string>() { "СС", "ССС" })
        );
        // з => ссс
        message = Regex3.Replace(
            message,
            _random.Pick(new List<string>() { "сс", "ссс" })
        );
        // З => CCC
        message = Regex4.Replace(
            message,
            _random.Pick(new List<string>() { "СС", "ССС" })
        );
        // ш => шшш
        message = Regex5.Replace(
            message,
            _random.Pick(new List<string>() { "шш", "шшш" })
        );
        // Ш => ШШШ
        message = Regex6.Replace(
            message,
            _random.Pick(new List<string>() { "ШШ", "ШШШ" })
        );
        // ч => щщщ
        message = Regex7.Replace(
            message,
            _random.Pick(new List<string>() { "щщ", "щщщ" })
        );
        // Ч => ЩЩЩ
        message = Regex8.Replace(
            message,
            _random.Pick(new List<string>() { "ЩЩ", "ЩЩЩ" })
        );
        // Corvax-Localization-End
        args.Message = message;
    }
}
