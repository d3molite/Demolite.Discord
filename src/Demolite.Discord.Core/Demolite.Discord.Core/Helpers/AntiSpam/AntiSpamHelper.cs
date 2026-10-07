using Demolite.Discord.Core.Configuration;
using NetCord.Gateway;
using Serilog;

namespace Demolite.Discord.Core.Helpers.AntiSpam;

public class AntiSpamHelper(GuildConfig[] guildConfigs, SpamPunisher punisher)
{
	private const int MaxMessagesPerUser = 10;

	private static readonly TimeSpan MessageLifetime = TimeSpan.FromMinutes(5);
	private static readonly TimeSpan DeleteDelay = TimeSpan.FromSeconds(2);

	private readonly object _lock = new();
	private readonly Dictionary<GuildUser, List<Message>> _recentMessages = [];
	private readonly Dictionary<GuildUser, List<Message>> _pendingPunishments = [];

	private readonly HashSet<ulong> _honeyPots = [..guildConfigs.Select(x => x.GuardConfig?.HoneyPotChannelId).OfType<ulong>()];

	/// <summary>
	/// Tracks the message and starts a single punishment per user as soon as spam is detected.
	/// </summary>
	public void CheckForSpam(Message message)
	{
		if (message.GuildId is not { } guildId || IsExcluded(message))
			return;

		lock (_lock)
		{
			var key = new GuildUser(guildId, message.Author.Id);

			if (_pendingPunishments.TryGetValue(key, out var pending))
			{
				pending.Add(message);
				return;
			}

			var recent = GetRecentMessages(key);
			recent.Add(message);

			if (recent.Count > MaxMessagesPerUser)
				recent.RemoveAt(0);

			if (!IsSpam(recent))
				return;

			_recentMessages.Remove(key);
			_pendingPunishments[key] = recent;
			_ = PunishAfterDelayAsync(key, message.Author, recent);
		}
	}

	/// <summary>
	/// Removes expired messages and users without remaining messages.
	/// </summary>
	public void RemoveExpiredMessages()
	{
		lock (_lock)
		{
			var oldestAllowed = DateTimeOffset.UtcNow - MessageLifetime;

			foreach (var (key, messages) in _recentMessages)
			{
				messages.RemoveAll(x => x.CreatedAt < oldestAllowed);

				if (messages.Count == 0)
					_recentMessages.Remove(key);
			}
		}
	}

	private async Task PunishAfterDelayAsync(GuildUser key, NetCord.User user, List<Message> pending)
	{
		try
		{
			// Collect messages which arrive while the spam is still running.
			await Task.Delay(DeleteDelay);

			await punisher.TimeOutAsync(key.GuildId, user);

			var batch = TakeUnhandledMessages(key, pending);

			while (batch.Length > 0)
			{
				await punisher.DeleteMessagesAsync(batch);
				batch = TakeUnhandledMessages(key, pending);
			}

			Log.Information("Spam detected by {User}", user);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Could not punish spam by {User}", user);
		}
	}

	/// <summary>
	/// Removes and returns the pending messages. Ends the pending state once none are left.
	/// </summary>
	private Message[] TakeUnhandledMessages(GuildUser key, List<Message> pending)
	{
		lock (_lock)
		{
			var batch = pending.ToArray();
			pending.Clear();

			if (batch.Length == 0)
				_pendingPunishments.Remove(key);

			return batch;
		}
	}

	private List<Message> GetRecentMessages(GuildUser key)
	{
		if (_recentMessages.TryGetValue(key, out var messages))
			return messages;

		return _recentMessages[key] = [];
	}

	private bool IsSpam(List<Message> messages)
		=> messages.Any(x => _honeyPots.Contains(x.ChannelId)) || SpamDetector.IsSpam(messages);

	private bool IsExcluded(Message message)
	{
		var config = guildConfigs.FirstOrDefault(x => x.Id == message.GuildId);

		return config?.GuardConfig?.AntispamExceptions.Any(x => message.Content.StartsWith(x)) == true;
	}

	private readonly record struct GuildUser(ulong GuildId, ulong UserId);
}
