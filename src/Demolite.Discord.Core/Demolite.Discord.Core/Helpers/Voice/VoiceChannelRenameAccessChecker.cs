using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;

namespace Demolite.Discord.Core.Helpers.Voice;

public class VoiceChannelRenameAccessChecker(
    Dictionary<ulong, GuildConfig> guildConfigs,
    VoiceStateCache voiceStates,
    VoiceChannelCache createdChannels)
{
    public bool CanRename(ulong guildId, ulong channelId, ulong userId)
        => guildConfigs.IsVoiceChannelRenameAllowed(guildId)
           && createdChannels.Contains(channelId)
           && voiceStates.IsIn(userId, channelId);
}