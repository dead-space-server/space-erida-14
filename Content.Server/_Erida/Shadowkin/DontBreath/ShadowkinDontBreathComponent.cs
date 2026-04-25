using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.DontBreath;

[RegisterComponent]
public sealed partial class DontBreathComponent : Component
{
    public ProtoId<DamageTypePrototype> AsphyxiationPrototype = "Asphyxiation";
    public RespiratorDataStruct RespiratorData;

    public struct RespiratorDataStruct
    {
        public ProtoId<EmotePrototype>? GaspEmote;

        public DamageSpecifier Damage;

        public DamageSpecifier DamageRecovery;
    }

}
