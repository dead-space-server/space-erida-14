
using Robust.Shared.Audio;

namespace Content.Server._Erida.CkeyCode;

[RegisterComponent]
public sealed partial class CkeyCodeComponent : Component
{
    [DataField]
    public HashSet<string> Ckeys = new();

    [DataField]
    public SoundSpecifier DeniedSound = new SoundPathSpecifier("/Audio/_Erida/Effects/Electronical/glitch.ogg");

    [DataField]
    public SoundSpecifier AcceptedSound = new SoundPathSpecifier("/Audio/_Erida/Effects/Electronical/oscillations.ogg");

    [DataField, ViewVariables]
    public float ExplosionTime = 6.4f;

    [ViewVariables]
    public bool IsArmed = false;

    public int LastSecond;

    public EntityUid Wearer;
}
