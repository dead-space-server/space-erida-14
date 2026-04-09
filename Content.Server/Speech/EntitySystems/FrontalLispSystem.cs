using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random; // Corvax-Localization

namespace Content.Server.Speech.EntitySystems;

public sealed class FrontalLispSystem : EntitySystem
{
    // @formatter:off
    private static readonly Regex RegexUpperTh = new(@"[T]+[Ss]+|[S]+[Cc]+(?=[IiEeYy]+)|[C]+(?=[IiEeYy]+)|[P][Ss]+|([S]+[Tt]+|[T]+)(?=[Ii]+[Oo]+[Uu]*[Nn]*)|[C]+[Hh]+(?=[Ii]*[Ee]*)|[Z]+|[S]+|[X]+(?=[Ee]+)");
    private static readonly Regex RegexLowerTh = new(@"[t]+[s]+|[s]+[c]+(?=[iey]+)|[c]+(?=[iey]+)|[p][s]+|([s]+[t]+|[t]+)(?=[i]+[o]+[u]*[n]*)|[c]+[h]+(?=[i]*[e]*)|[z]+|[s]+|[x]+(?=[e]+)");
    private static readonly Regex RegexUpperEcks = new(@"[E]+[Xx]+[Cc]*|[X]+");
    private static readonly Regex RegexLowerEcks = new(@"[e]+[x]+[c]*|[x]+");

    // Erida start
    private static readonly Regex Regex1 = new(@"с");
    private static readonly Regex Regex2 = new(@"с");

    private static readonly Regex Regex3 = new(@"ч");
    private static readonly Regex Regex4 = new(@"ч");
    private static readonly Regex Regex5 = new(@"ц");
    private static readonly Regex Regex6 = new(@"ц");
    private static readonly Regex Regex7 = new(@"\B[т](?![АЕЁИОУЫЭЮЯаеёиоуыэюя])");
    private static readonly Regex Regex8 = new(@"\B[т](?![АЕЁИОУЫЭЮЯаеёиоуыэюя])");
    private static readonly Regex Regex9 = new(@"з");
    private static readonly Regex Regex10 = new(@"з");
    // Erida end
    // @formatter:on

    [Dependency] private readonly IRobustRandom _random = default!; // Corvax-Localization

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrontalLispComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, FrontalLispComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // handles ts, sc(i|e|y), c(i|e|y), ps, st(io(u|n)), ch(i|e), z, s
        message = RegexUpperTh.Replace(message, "TH");
        message = RegexLowerTh.Replace(message, "th");
        // handles ex(c), x
        message = RegexUpperEcks.Replace(message, "EKTH");
        message = RegexLowerEcks.Replace(message, "ekth");

        // Corvax-Localization Start
        // с - ш
        message = Regex1.Replace(message, _random.Prob(0.90f) ? "ш" : "с");
        message = Regex2.Replace(message, _random.Prob(0.90f) ? "Ш" : "С");
        // ч - ш
        message = Regex3.Replace(message, _random.Prob(0.90f) ? "ш" : "ч");
        message = Regex4.Replace(message, _random.Prob(0.90f) ? "Ш" : "Ч");
        // ц - ч
        message = Regex5.Replace(message, _random.Prob(0.90f) ? "ч" : "ц");
        message = Regex6.Replace(message, _random.Prob(0.90f) ? "Ч" : "Ц");
        // т - ч
        message = Regex7.Replace(message, _random.Prob(0.90f) ? "ч" : "т");
        message = Regex8.Replace(message, _random.Prob(0.90f) ? "Ч" : "Т");
        // з - ж
        message = Regex9.Replace(message, _random.Prob(0.90f) ? "ж" : "з");
        message = Regex10.Replace(message, _random.Prob(0.90f) ? "Ж" : "З");
        // Corvax-Localization End

        args.Message = message;
    }
}
