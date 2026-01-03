using Content.Server._Erida.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;

namespace Content.Server._Erida.Speech.EntitySystems
{
    public sealed class StreetRebelAccentSystem : EntitySystem
    {
        [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<StreetRebelAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            var msg = message;

            msg = _replacement.ApplyReplacements(msg, "streetrebel");

            return msg;
        }

        private void OnAccent(Entity<StreetRebelAccentComponent> ent, ref AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
