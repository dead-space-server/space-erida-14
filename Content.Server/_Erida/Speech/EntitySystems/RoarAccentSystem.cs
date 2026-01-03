using System.Text.RegularExpressions;
using Content.Server._Erida.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._Erida.Speech.EntitySystems;

public sealed class RoarAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoarAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, RoarAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // r > rrr / R > RRR
        message = Regex.Replace(message, "r+", _random.Pick(new List<string>() { "rr", "RRR" }));
        message = Regex.Replace(message, "R+", _random.Pick(new List<string>() { "RR", "RRR" }));

        // р > ррр / Р > РРР
        message = Regex.Replace(message, "р+", _random.Pick(new List<string>() { "рр", "ррр" }));
        message = Regex.Replace(message, "Р+", _random.Pick(new List<string>() { "РР", "РРР" }));

        args.Message = message;
    }
}
