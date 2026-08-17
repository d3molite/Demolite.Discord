using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Helpers.Voice;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers.Voice;

public class CustomVoiceChannelHandler(
    Dictionary<ulong, GuildConfig> guildConfigs,
    VoiceStateCache voiceStateCache,
    RestClient client,
    VoiceChannelCache createdChannels, 
    VoiceChannelCleaner cleaner) : IVoiceStateUpdateGatewayHandler
{
    private static readonly SemaphoreSlim CreateLock = new(1, 1);

    public async ValueTask HandleAsync(VoiceState arg)
    {
        if (!guildConfigs.TryGetTriggerChannelId(arg.GuildId, out var triggerChannelId))
            return;

        var previous = voiceStateCache.Update(arg);

        if (arg.ChannelId is { } joinedId)
        {
            if (joinedId == triggerChannelId && previous?.ChannelId != triggerChannelId)
                await CreateAndMoveAsync(arg, triggerChannelId);
            else
                createdChannels.CancelPendingDelete(joinedId);
        }

        if (previous is not null)
            ScheduleDeleteIfAbandoned(previous, arg);
    }

    private async Task CreateAndMoveAsync(VoiceState voiceState, ulong triggerChannelId)
    {
        var channel = await CreateChannelAsync(voiceState, triggerChannelId);

        cleaner.ScheduleDelete(channel.Id);
        await PostRenameMessageIfAllowedAsync(voiceState.GuildId, channel.Id);
        await client.ModifyGuildUserAsync(voiceState.GuildId, voiceState.UserId, o => o.ChannelId = channel.Id);
    }
    
    private void ScheduleDeleteIfAbandoned(VoiceState previous, VoiceState current)
    {
        if (previous.ChannelId is not { } leftId || leftId == current.ChannelId)
            return;

        if (createdChannels.Contains(leftId) && voiceStateCache.CountIn(leftId) == 0)
            cleaner.ScheduleDelete(leftId);
    }

    private async Task<IGuildChannel> CreateChannelAsync(VoiceState voiceState, ulong triggerChannelId)
    {
        await CreateLock.WaitAsync();
        try
        {
            var trigger = (VoiceGuildChannel)await client.GetChannelAsync(triggerChannelId);
            var number = createdChannels.GetFreeNumber(voiceState.GuildId);
            var name = VoiceChannelName.Format(number, voiceState.User?.Username ?? voiceState.UserId.ToString());
            
            var channel = await client.CreateGuildChannelAsync(voiceState.GuildId,
                new GuildChannelProperties(name, ChannelType.VoiceGuildChannel) { ParentId = trigger.ParentId });

            createdChannels.Add(channel.Id, voiceState.GuildId, number);
            return channel;
        }
        finally
        {
            CreateLock.Release();
        }
    }
    
    private async Task PostRenameMessageIfAllowedAsync(ulong guildId, ulong channelId)
    {
        if (!guildConfigs.IsVoiceChannelRenameAllowed(guildId))
            return;

        var culture = guildConfigs.GetLoggingCulture(guildId);
        await client.SendMessageAsync(channelId, VoiceChannelRenameMessageBuilder.Create(culture));
    }
}