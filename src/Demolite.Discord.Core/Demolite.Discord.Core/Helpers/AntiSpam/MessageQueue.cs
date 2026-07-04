using System.Text;
using NetCord.Gateway;
using Serilog;

namespace Demolite.Discord.Core.Helpers.AntiSpam;

public class MessageQueue(int maxMessages)
{
	private TimeSpan LifeTimeMinutes => TimeSpan.FromMinutes(5);

	public List<Message> Queue { get; private set; } = [];

	public bool IsEmpty => !Queue.Any();

	/// <summary>
	/// Enqueue a message into the queue.
	/// After <see cref="maxMessages" /> has been reached, the oldest message will be deleted.
	/// </summary>
	/// <param name="message">Message to enqueue</param>
	public void Enqueue(Message message)
	{
		if (Queue.Count >= maxMessages)
		{
			Queue.RemoveAt(0);
			Log.Verbose("Queue is full, removed first message");
		}

		Queue.Add(message);
		Log.Verbose("Enqueued message: {Message}", message);
	}

	public Message Dequeue()
	{
		var item = Queue[0];
		Queue.RemoveAt(0);
		return item;
	}

	/// <summary>
	/// Forces a message into the queue, regardless of the queue size.
	/// </summary>
	/// <param name="message">Message to enqueue</param>
	public void ForceEnqueue(Message message)
	{
		if (!Queue.Contains(message))
			Queue.Add(message);
	}

	/// <summary>
	/// Clears the message queue.
	/// </summary>
	public void Clear()
	{
		Queue.Clear();
	}

	/// <summary>
	/// Method which removes old items from the queue.
	/// </summary>
	public void DequeueOldItems()
	{
		try
		{
			var keepMessages = Queue.Where(message => DateTime.Now - message.CreatedAt < LifeTimeMinutes);
			Queue = keepMessages.ToList();
		}
		catch (InvalidOperationException ex)
		{
			Log.Error("Error while removing from queue: {Exception}", ex.Message);
		}
	}

	/// <summary>
	/// Lists all messages in the queue.
	/// </summary>
	/// <returns>A list of all queued messages</returns>
	public override string ToString()
	{
		var sb = new StringBuilder();

		foreach (var message in Queue) sb.AppendLine($"{message.Author} at {message.CreatedAt}: {message.Content}");

		return sb.ToString();
	}
}