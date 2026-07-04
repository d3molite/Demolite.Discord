using System.Diagnostics.CodeAnalysis;
using NetCord;
using NetCord.Rest;
using Serilog;

namespace Demolite.Discord.Core.Helpers.AntiSpam;


public class SpamHandler
{
	private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(2));
	
	private readonly Dictionary<string, List<string>> _deletedMessages = new();

	[method: SetsRequiredMembers]
	public SpamHandler(RestClient restClient, RestGuild guild, User user, MessageQueue queue)
	{
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
	
		while (!MessageQueue.IsEmpty)
		{
			var nextMessage = MessageQueue.Dequeue();
			var guildChannel = _channels.First(x => x.Id == nextMessage.ChannelId);
			await nextMessage.DeleteAsync();
			
			if (_deletedMessages.TryGetValue(guildChannel.Name, out var messages))
				messages.Add(nextMessage.Content);
			else
				_deletedMessages.Add(guildChannel.Name, [nextMessage.Content]);
			
			Thread.Sleep(200);
		}
	
		await SendLog();
		SpamDeleted?.Invoke(this, EventArgs.Empty);
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
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Could not time out user {User}", User);
		}
	}
}