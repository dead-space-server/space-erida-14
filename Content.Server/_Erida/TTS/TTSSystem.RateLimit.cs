using Content.Server.Chat.Managers;
using Content.Server.Players.RateLimiting;
using Content.Shared.CCVar;
using Content.Shared.Players.RateLimiting;
using Robust.Shared.Player;

namespace Content.Server._Erida.TTS;

public sealed partial class TTSSystem
{
    [Dependency] private PlayerRateLimitManager _rateLimitManager = default!;
    [Dependency] private IChatManager _chat = default!;

    private const string RateLimitKey = "TTS";

    private void RegisterRateLimits()
    {
        _rateLimitManager.Register(RateLimitKey,
            new RateLimitRegistration(
                CCVars.TTSRateLimitPeriod,
                CCVars.TTSRateLimitCount,
                RateLimitPlayerLimited)
            );
    }

    private void RateLimitPlayerLimited(ICommonSession player)
    {
        _chat.DispatchServerMessage(player, Loc.GetString("tts-rate-limited"), suppressLog: true);
    }

    private RateLimitStatus HandleRateLimit(ICommonSession player)
    {
        return _rateLimitManager.CountAction(player, RateLimitKey);
    }
}
