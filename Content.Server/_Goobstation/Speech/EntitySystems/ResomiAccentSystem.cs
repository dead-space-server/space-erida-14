using System.Text.RegularExpressions;
using Content.Server._Goobstation.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Speech.EntitySystems;

public sealed class ResomiAccentSystem : EntitySystem
{
    // Erida-start
     private static readonly Regex Regex1 = new("ш+");
    private static readonly Regex Regex2 = new("Ш+");
    private static readonly Regex Regex3 = new("ч+");
    private static readonly Regex Regex4 = new("Ч+");
    private static readonly Regex Regex5 = new("р+");
    private static readonly Regex Regex6 = new("Р+");
    // Erida-end

    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ResomiAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, ResomiAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // ш => шшш
        message = Regex1.Replace(
            message,
            _random.Pick(new List<string>() { "шш", "шшш" })
        );
        // Ш => ШШШ
        message = Regex2.Replace(
            message,
            _random.Pick(new List<string>() { "ШШ", "ШШШ" })
        );
        // ч => щщщ
        message = Regex3.Replace(
            message,
            _random.Pick(new List<string>() { "щщ", "щщщ" })
        );
        // Ч => ЩЩЩ
        message = Regex4.Replace(
            message,
            _random.Pick(new List<string>() { "ЩЩ", "ЩЩЩ" })
        );
        // р => ррр
        message = Regex5.Replace(
            message,
            _random.Pick(new List<string>() { "рр", "ррр" })
        );
        // Р => РРР
        message = Regex6.Replace(
            message,
            _random.Pick(new List<string>() { "РР", "РРР" })
        );
        args.Message = message;
    }
}
