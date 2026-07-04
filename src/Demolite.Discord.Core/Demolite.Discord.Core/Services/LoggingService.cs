using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Interfaces;
using NetCord;
using NetCord.JsonModels;
using NetCord.Rest;

namespace Demolite.Discord.Core.Services;

public class LoggingService(RestClient restClient, List<GuildConfig> guildConfigs, ChannelMessageCache cache)
	: ILoggingService
{
	private const string MessageDeleted = "A message by user <@{0}> ({1}) was deleted in channel <#{2}>.";

	public async Task LogDefault(ulong guildId, EmbedProperties[] embed)
	{
		throw new NotImplementedException();
	}

	public async Task LogCritical(ulong guildId, EmbedProperties[] embed)
	{
		var channelId = GetLoggingChannelId(guildId, true);
		var channel = await restClient.GetChannelAsync(channelId);

		if (channel is TextChannel textChannel)
			await textChannel.SendMessageAsync(embed.CreateLogMessage());
	}

	public async Task LogMessageDeleted(ulong guildId, ulong channelId, ulong messageId)
	{
		var messageDeleted = "The content could not be fetched from the cache.";
		var messageInfo = $"A message was deleted in channel <#{channelId}>.";

		if (cache.TryRemove(channelId, messageId, out var message))
		{
			messageDeleted = message.Content;
			messageInfo = string.Format(MessageDeleted, message?.Author.Id, message?.Author.Username, channelId);
		}

		JsonEmbedField[] fields =
			[
				new()
				{
					Name = "Message deleted",
					Value = messageInfo,
					Inline = false
				},
				new()
				{
					Name = "Content:",
					Value = messageDeleted,
					Inline = false
				}
			];

			await LogCritical(guildId, [fields.CreateLogEmbed()]);
		}

		private ulong GetLoggingChannelId(ulong guildId, bool isCritical)
		{
			var guildConfig = guildConfigs.FirstOrDefault(g => g.Id == guildId);

			if (guildConfig == null)
				throw new Exception($"Guild config not found for guild ID: {guildId}");

			return isCritical ? guildConfig.LogCritical : guildConfig.LogDefault;
		}
	}