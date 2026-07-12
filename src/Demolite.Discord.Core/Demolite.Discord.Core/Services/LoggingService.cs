using System.Resources;
using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Interfaces;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using Serilog;

namespace Demolite.Discord.Core.Services;

public partial class LoggingService(
	RestClient restClient,
	GatewayClient gatewayClient,
	Dictionary<ulong, GuildConfig> guildConfigs,
	ChannelMessageCache cache
) : ILoggingService
{
	private const string MessageDeleted = "A message by user <@{0}> ({1}) was deleted in channel <#{2}>.";

	private static ResourceManager Resources => LoggingResource.ResourceManager;

	public async Task LogDefault(ulong guildId, EmbedProperties[] embed)
	{
		var channelId = GetLoggingChannelId(guildId, false);
		var channel = await restClient.GetChannelAsync(channelId);

		if (channel is TextChannel textChannel)
			await textChannel.SendMessageAsync(embed.CreateLogMessage());
		else
			Log.Error("Logging channel with id '{ChannelId}' is not a text channel", channelId);
	}

	public async Task LogCritical(ulong guildId, EmbedProperties[] embed)
	{
		var channelId = GetLoggingChannelId(guildId, true);
		var channel = await restClient.GetChannelAsync(channelId);

		if (channel is TextChannel textChannel)
			await textChannel.SendMessageAsync(embed.CreateLogMessage());
		else
			Log.Error("Logging channel with id '{ChannelId}' is not a text channel", channelId);
	}

	private ulong GetLoggingChannelId(ulong guildId, bool isCritical)
	{
		if (!guildConfigs.TryGetValue(guildId, out var guildConfig))
			throw new KeyNotFoundException($"The specified guild id '{guildId}' was not found.");

		return isCritical ? guildConfig.LogCritical : guildConfig.LogDefault;
	}

	private string? GetLoggingCulture(ulong guildId)
	{
		if (!guildConfigs.TryGetValue(guildId, out var guildConfig))
			throw new KeyNotFoundException($"The specified guild id '{guildId}' was not found.");

		return guildConfig.LoggingCulture;
	}
}