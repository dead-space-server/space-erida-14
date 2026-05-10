using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<string> DiscordReadminInfoWebhook =
        CVarDef.Create("discord.readmin_info_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> DiscordReadminInfoEmbedColor =
        CVarDef.Create("discord.readmin_info_embed_color", Color.DeepPink.ToHex(), CVar.SERVERONLY);

    public static readonly CVarDef<string> DiscordReadminInfoEmbedColorDebug =
        CVarDef.Create("discord.readmin_info_embed_color_debug", Color.Gray.ToHex(), CVar.SERVERONLY);

    public static readonly CVarDef<bool> DiscordReadminInfoIsActive =
        CVarDef.Create("discord.readmin_info_is_active", true, CVar.SERVERONLY);
}
