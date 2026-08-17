using Demolite.Discord.Core.Configuration;

namespace Demolite.Discord.Core.Extensions;

public static class GuildConfigExtensions
{
    public static bool TryGetTriggerChannelId(
        this Dictionary<ulong, GuildConfig> guildConfigs, ulong guildId, out ulong triggerChannelId)
    {
        triggerChannelId = guildConfigs.GetValueOrDefault(guildId)?.ToolsConfig?.VoiceChannelCreateId ?? 0;
        return triggerChannelId != 0;
    }

    public static string? GetLoggingCulture(this Dictionary<ulong, GuildConfig> guildConfigs, ulong guildId)
        => guildConfigs.GetValueOrDefault(guildId)?.LoggingCulture;

    public static bool IsVoiceChannelRenameAllowed(this Dictionary<ulong, GuildConfig> guildConfigs, ulong guildId)
        => guildConfigs.GetValueOrDefault(guildId)?.ToolsConfig?.VoiceChannelRenameAllowed == true;

    public static TimeSpan GetVoiceChannelDeleteDelay(this Dictionary<ulong, GuildConfig> guildConfigs, ulong guildId)
    {
        var toolsConfig = guildConfigs.GetValueOrDefault(guildId)?.ToolsConfig ?? new ToolsConfig();
        return TimeSpan.FromSeconds(toolsConfig.VoiceChannelDeleteDelaySeconds);
    }
}