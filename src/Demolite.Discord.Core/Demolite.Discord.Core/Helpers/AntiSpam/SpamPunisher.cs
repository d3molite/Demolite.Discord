using Demolite.Discord.Core.Interfaces;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using Serilog;

namespace Demolite.Discord.Core.Helpers.AntiSpam;

public class SpamPunisher(RestClient restClient, ILoggingService loggingService)
{
	private const int MaxBulkDeleteSize = 100;

	private static readonly TimeSpan TimeoutDuration = TimeSpan.FromDays(3);

	/// <summary>
	/// Deletes the messages, one bulk request per channel and chunk.
	/// </summary>
	public async Task DeleteMessagesAsync(IEnumerable<Message> messages)
	{
		foreach (var channelMessages in messages.GroupBy(x => x.ChannelId))
		{
			var messageIds = channelMessages.Select(x => x.Id).Distinct();
			await DeleteMessagesAsync(channelMessages.Key, messageIds);
		}
	}

	/// <summary>
	/// Times out the user and logs it.
	/// </summary>
	public async Task TimeOutAsync(ulong guildId, User user)
	{
		try
		{
			var timeoutEnd = DateTimeOffset.UtcNow.Add(TimeoutDuration);
			var properties = new RestRequestProperties { AuditLogReason = "Spam detected" };

			await restClient.ModifyGuildUserAsync(guildId, user.Id, x => x.TimeOutUntil = timeoutEnd, properties);
			await loggingService.LogUserTimedOut(guildId, user);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Could not time out user {User}", user);
		}
	}

	private async Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds)
	{
		foreach (var chunk in messageIds.Chunk(MaxBulkDeleteSize))
		{
			try
			{
				if (chunk.Length == 1)
					await restClient.DeleteMessageAsync(channelId, chunk[0]);
				else
					await restClient.DeleteMessagesAsync(channelId, chunk);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Could not delete spam messages in channel {ChannelId}", channelId);
			}
		}
	}
}
