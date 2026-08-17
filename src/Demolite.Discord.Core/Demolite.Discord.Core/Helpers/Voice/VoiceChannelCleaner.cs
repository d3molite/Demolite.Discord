using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using NetCord.Rest;

namespace Demolite.Discord.Core.Helpers.Voice;

public sealed class VoiceChannelCleaner(
    Dictionary<ulong, GuildConfig> guildConfigs,
    RestClient client,
    VoiceStateCache voiceStateCache,
    VoiceChannelCache createdChannels)
{
    public void ScheduleDelete(ulong guildId, ulong channelId)
        => _ = DeleteAfterDelayAsync(
            channelId,
            guildConfigs.GetVoiceChannelDeleteDelay(guildId),
            createdChannels.StartPendingDelete(channelId));

    private async Task DeleteAfterDelayAsync(ulong channelId, TimeSpan delay, CancellationToken token)
    {
        try
        {
            await Task.Delay(delay, token);

            if (voiceStateCache.CountIn(channelId) > 0)
                return;

            await client.DeleteChannelAsync(channelId, cancellationToken: token);
            createdChannels.Remove(channelId);
        }
        catch (OperationCanceledException)
        {
            // do nothing here
        }
    }
}