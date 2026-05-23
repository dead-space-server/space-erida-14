using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Content.Server.Administration.Managers;
using Content.Server.Discord;
using Content.Shared.Audio.Jukebox;
using Content.Shared.CCVar;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Utility;

public sealed partial class ReadminLogging : EntitySystem
{
    [Dependency] private DiscordWebhook _discord = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IAdminManager _adminManager = default!;
    private WebhookIdentifier? _webhookId = null;
    private Color _webhookEmbedColor;
    private Color _webhookEmbedColorDebug;
    private bool _readminInfoActive;
    private Dictionary<string, DateTime> _adminTimeStats = [];

    // Sorry, but shitcode
    private readonly List<string> _adminRankWhitelist =
    [
        "Руководитель Администрациии",
        "Старший Администратор",
        "Администратор",
        "Младший Администратор"
    ];

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCVars.DiscordReadminInfoWebhook,
            value =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _discord.GetWebhook(value, data => _webhookId = data.ToIdentifier());
            }, true);

        _cfg.OnValueChanged(CCVars.DiscordReadminInfoEmbedColor, value =>
            {
                _webhookEmbedColor = Color.LawnGreen;
                if (Color.TryParse(value, out var color))
                    _webhookEmbedColor = color;
            }, true);

        _cfg.OnValueChanged(CCVars.DiscordReadminInfoEmbedColorDebug, value =>
            {
                _webhookEmbedColorDebug = Color.LawnGreen;
                if (Color.TryParse(value, out var color))
                    _webhookEmbedColorDebug = color;
            }, true);


        _cfg.OnValueChanged(CCVars.DiscordReadminInfoIsActive, value => _readminInfoActive = value, true);
    }

    private sealed class ReadminLoggingInformation()
    {
        public string UserName = string.Empty;
        public bool IsAdmin;
        public bool InGame;
        public TimeSpan OnServerTime;
        public TimeSpan AdminnedTime;
    }

    internal void TryDiscordLog(ICommonSession session, bool isAdmin)
    {
        if (!_readminInfoActive || _webhookId == null)
            return;

        var adminData = _adminManager.GetAdminData(session, true);
        var rankName = adminData?.Title;

        if (rankName == null)
            return;

        if (!_adminRankWhitelist.Contains(rankName!))
            return;

        var adminnedTime = TimeSpan.Zero;

        if (isAdmin)
        {
            _adminTimeStats.TryAdd(session.UserId.ToString(), DateTime.UtcNow);
        }
        else
        {
            _adminTimeStats.TryGetValue(session.UserId.ToString(), out var dateTime);
            adminnedTime = DateTime.UtcNow - dateTime;
            _adminTimeStats.Remove(session.UserId.ToString());
        }

        var information = new ReadminLoggingInformation()
        {
            UserName = session.Name,
            IsAdmin = isAdmin,
            InGame = SessionStatus.InGame == session.Status,
            OnServerTime = DateTime.Now - session.ConnectedTime,
            AdminnedTime = adminnedTime
        };

        _ = SendArticleToDiscordWebhook(information);
    }

    private async Task SendArticleToDiscordWebhook(ReadminLoggingInformation information)
    {
        if (_webhookId is null)
            return;

        var isConclusion = !information.InGame || !information.IsAdmin;

        var description = string.Empty;

        var isInGameLocale = information.InGame ? Loc.GetString("readmin-logging-leaved-not-in-game")
                : Loc.GetString("readmin-logging-leaved-in-game");

        if (isConclusion)
            description = Loc.GetString("readmin-logging-leaved-from-game",
                ("time", information.AdminnedTime.Minutes), ("onServer", isInGameLocale), ("user", information.UserName));
        else
            description = Loc.GetString("readmin-logging-joined-to-game", ("onServer", isInGameLocale), ("user", information.UserName));

        var embed = new WebhookEmbed
        {
            Description = description,
            Color = isConclusion ? _webhookEmbedColor.ToArgb() & 0xFFFFFF : _webhookEmbedColorDebug.ToArgb() & 0xFFFFFF,
            Footer = new WebhookEmbedFooter
            {
                Text = Loc.GetString("readmin-logging-on-server-time", ("time", information.OnServerTime.Minutes))
            }
        };

        var payload = new WebhookPayload { Embeds = [embed] };

        await _discord.CreateMessage(_webhookId.Value, payload);
    }

}
