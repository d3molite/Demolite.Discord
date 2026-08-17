using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Helpers.Voice;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Serilog;

namespace Demolite.Discord.Core.Bot.Handlers.Voice;

public partial class VoiceChannelRecoveryHandler(
    Dictionary<ulong, GuildConfig> guildConfigs,
    VoiceStateCache voiceStateCache,
    VoiceChannelCache createdChannels,
    VoiceChannelCleaner cleaner) : IGuildCreateGatewayHandler
{
    [GeneratedRegex(@"^(\d+) \| ")]
    private static partial Regex NumberPrefix();

    public ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        if (arg.Guild is not { } guild || !TryGetTriggerChannel(guild, out var trigger))
            return default;

        foreach (var state in guild.VoiceStates.Values)
            voiceStateCache.Update(state);

        var candidates = guild.Channels.Values
            .OfType<VoiceGuildChannel>()
            .Where(c => c.Id != trigger.Id && c.ParentId == trigger.ParentId);

        foreach (var candidate in candidates)
            Recover(guild.Id, candidate);

        return default;
    }

    private void Recover(ulong guildId, VoiceGuildChannel channel)
    {
        if (!VoiceChannelName.TryParseNumber(channel.Name, out var number))
            return;

        createdChannels.Add(channel.Id, guildId, number);

        var isEmpty = voiceStateCache.CountIn(channel.Id) == 0;
        Log.Information(
            "Recovered channel {ChannelId} ({ChannelName}) with number {Number} in guild {GuildId}, empty: {IsEmpty}",
            channel.Id, channel.Name, number, guildId, isEmpty);

        if (isEmpty)
            cleaner.ScheduleDelete(channel.Id);
    }
    
    private bool TryGetTriggerChannel(Guild guild, [NotNullWhen(true)] out VoiceGuildChannel? trigger)
    {
        trigger = null;

        if (!guildConfigs.TryGetTriggerChannelId(guild.Id, out var triggerId))
            return false;

        trigger = guild.Channels.GetValueOrDefault(triggerId) as VoiceGuildChannel;
        return trigger is not null;
    }
}