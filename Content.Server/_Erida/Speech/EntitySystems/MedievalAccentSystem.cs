using Content.Server._Erida.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;

namespace Content.Server._Erida.Speech.EntitySystems
{
    public sealed class MedievalAccentSystem : EntitySystem
    {
        [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MedievalAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            var msg = message;

            msg = _replacement.ApplyReplacements(msg, "medieval"); 

            return msg;
        }

        private void OnAccent(Entity<MedievalAccentComponent> ent, ref AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
