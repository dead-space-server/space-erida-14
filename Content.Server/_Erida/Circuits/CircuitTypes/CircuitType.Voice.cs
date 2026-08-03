using Content.Server.Chat.Systems;
using Content.Shared._Erida.Circuits.Components;
using Content.Shared._Erida.TTS;
using Content.Shared.Speech;

#pragma warning disable CS8629

namespace Content.Server._Erida.Circuits;

public sealed partial class ServerCircuitSystem
{
    private void OnSignalVoice(Entity<CircuitComponentComponent> ent)
    {
        if (!EnsureAllPortsHaveValue([ent.Comp.Inputs[0]], ent.Owner))
            return;

        if (!ConsumePowerIfEnough(ent.Comp.NetContainer, ent.Comp.PowerConsuming))
            return;

        var owner = GetEntity(ent.Comp.NetContainer);

        if (owner == null)
            return;

        var ttsComp = EnsureComp<TTSComponent>(owner.Value);
        ttsComp.VoicePrototypeId = "Sunboy_inner";

        EnsureComp<SpeechComponent>(owner.Value);

        _entityManager.System<ChatSystem>().TrySendInGameICMessage(owner.Value, ent.Comp.Inputs[0].Data.Value.String[0], Shared.Chat.InGameICChatType.Speak, false);

        SendSignalFromPort(ent.Comp.Output[0]);
    }
}
