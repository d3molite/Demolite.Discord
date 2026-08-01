using System.Diagnostics.CodeAnalysis;
using Demolite.Discord.Core.Interfaces;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using Serilog;

namespace Demolite.Discord.Core.Helpers.AntiSpam;


public class SpamHandler
{
	private readonly ILoggingService _loggingService;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(2));
	
	[method: SetsRequiredMembers]
	public SpamHandler(RestClient restClient, RestGuild guild, User user, MessageQueue queue, ILoggingService loggingService)
	{
		_loggingService = loggingService;
		Client = restClient;
		Guild = guild;
		User = user;
		MessageQueue = queue;
		
		Task.Run(async () => await TimerTask());
	}

	public event EventHandler? SpamDeleted;

	public required RestClient Client { get; init; }

	public required RestGuild Guild { get; init; }

	public required User User { get; init; }

	public required MessageQueue MessageQueue { get; init; }
	
	public required ILoggingService Logging { get; init; }

	private IReadOnlyList<IGuildChannel> _channels = [];
	
	private async Task TimerTask()
	{
		// Wait a few seconds before deleting the spam.
		_channels = await Guild.GetChannelsAsync();
		await _timer.WaitForNextTickAsync();
		await DeleteSpam();
	}

	private async Task DeleteSpam()
	{
		await TimeoutUser();

		foreach (var group in MessageQueue.Queue.GroupBy(m => m.ChannelId))
		{
			if (_channels.FirstOrDefault(x => x.Id == group.Key) is TextGuildChannel guildChannel)
				await DeleteMessagesBulk(guildChannel, group);
			else
				await DeleteMessagesIndividually(group);
		}

		MessageQueue.Clear();
		await SendLog();
		SpamDeleted?.Invoke(this, EventArgs.Empty);
	}
	
	private static async Task DeleteMessagesBulk(TextGuildChannel guildChannel, IEnumerable<Message> messages)
	{
		var ids = messages.Select(m => m.Id).ToArray();

		foreach (var chunk in ids.Chunk(100))
		{
			if (chunk.Length == 1)
				await guildChannel.DeleteMessageAsync(chunk[0]);
			else
				await guildChannel.DeleteMessagesAsync(chunk);
		}
	}

	private static async Task DeleteMessagesIndividually(IEnumerable<Message> messages)
	{
		foreach (var message in messages)
			await message.DeleteAsync();
	}
	
	private async Task SendLog()
	{
		Log.Information("Spam detected by {User}", User);
	}
	
	private async Task TimeoutUser()
	{
		try
		{
			var guild = await Client.GetGuildAsync(Guild.Id);
			var user = await guild.GetUserAsync(User.Id);
			
			await user.TimeOutAsync(DateTimeOffset.UtcNow.AddDays(3), properties: new RestRequestProperties()
			{
				AuditLogReason = $"Spam detected by {(await Client.GetCurrentUserAsync()).Username}"
			});
			
			await _loggingService.LogUserTimedOut(Guild.Id, User);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Could not time out user {User}", User);
		}
	}
}