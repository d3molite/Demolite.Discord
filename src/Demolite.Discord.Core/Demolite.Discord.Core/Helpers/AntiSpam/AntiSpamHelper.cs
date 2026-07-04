using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Helpers.AntiSpam;

public class AntiSpamHelper(RestClient restClient, RestGuild guild)
{
	private Dictionary<ulong, MessageQueue> _userMessages = [];
	
	private readonly List<SpamHandler> _spamHandlers = [];

	public void CleanupQueues()
	{
		foreach (var kvp in _userMessages)
		{
			kvp.Value.DequeueOldItems();
		}
	}

	public Task CheckForSpam(Message message)
	{
		var existingHandler = _spamHandlers.FirstOrDefault(x => x.User == message.Author);

		if (existingHandler != null)
		{
			AddToRunning(existingHandler, message);
			return Task.CompletedTask;
		}

		EnqueueMessage(message);
		return Task.CompletedTask;
	}
	
	private static void AddToRunning(SpamHandler existingHandler, Message message)
		=> existingHandler.MessageQueue.ForceEnqueue(message);
	
	private void EnqueueMessage(Message message)
	{
		if (_userMessages.TryGetValue(message.Author.Id, out var messageQueue))
		{
			messageQueue.Enqueue(message);
			CheckQueueForSpam(messageQueue, message.Author);
			return;
		}

		var queue = new MessageQueue(10);
		queue.Enqueue(message);
		_userMessages.Add(message.Author.Id, queue);
	}

	private void CheckQueueForSpam(MessageQueue queue, User user)
	{
		if (!ContainsSpam(queue))
			return;

		var handler = new SpamHandler(restClient, guild, user, queue);
		
		handler.SpamDeleted += Cleanup;
		_spamHandlers.Add(handler);
	}
	
	private void Cleanup(object? sender, EventArgs e)
	{
		if (sender is SpamHandler handler)
			_spamHandlers.Remove(handler);
	}
	
	private static bool ContainsSpam(MessageQueue queue)
	{
		var isSpamByMessage = queue.Queue
			.Where(x => !string.IsNullOrEmpty(x.Content))
			.GroupBy(message => message.Content)
			.Any(group => group.Count() > 5);
		
		var isSpamByAttachment = queue.Queue
			.Where(message => message.Attachments.Count > 0)
			.GroupBy(message => string.Join("", message.Attachments.Select(x => x.FileName)))
			.Any(group => group.Count() > 5);
		
		var isSpamByStickers = queue.Queue.Where(message => message.Stickers.Count > 0)
			.GroupBy(message => message.Stickers.First().Id)
			.Any(group => group.Count() > 5);

		return isSpamByMessage || isSpamByAttachment || isSpamByStickers;
	}
}