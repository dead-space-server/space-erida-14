using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private static readonly Regex RegexLowerBuzz = new Regex("z{1,3}");
    private static readonly Regex RegexUpperBuzz = new Regex("Z{1,3}");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // buzzz
        message = RegexLowerBuzz.Replace(message, "zzz");
        // buZZZ
        message = RegexUpperBuzz.Replace(message, "ZZZ");

        // Erida-Loc-Start
        // ж > жжж
        message = Regex.Replace(message, "ж+", _random.Pick(new List<string>() { "жж", "жжж" }));
        message = Regex.Replace(message, "Ж+", _random.Pick(new List<string>() { "ЖЖ", "ЖЖЖ" }));

        // з > ззз
        message = Regex.Replace(message, "з+", _random.Pick(new List<string>() { "зз", "ззз" }));
        message = Regex.Replace(message, "З+", _random.Pick(new List<string>() { "ЗЗ", "ЗЗЗ" }));
        // Erida-Loc-End

        args.Message = message;
    }
}
