using NetCord.Gateway;

namespace Demolite.Discord.Core.Helpers.Cache;

internal sealed class MessageCache(int capacity)
{
	private readonly Dictionary<ulong, Message> _messages = new();
	private readonly Queue<ulong> _order = new();
	private readonly Lock _lock = new();

	public void Add(Message message)
	{
		lock (_lock)
		{
			if (!_messages.TryAdd(message.Id, message))
				return;

			_order.Enqueue(message.Id);

			while (_messages.Count > capacity)
			{
				var oldest = _order.Dequeue();
				_messages.Remove(oldest);
			}
		}
	}

	public bool TryGet(ulong id, out Message? message)
	{
		lock (_lock)
		{
			return _messages.TryGetValue(id, out message);
		}
	}

	public bool TryRemove(ulong id, out Message? message)
	{
		lock (_lock)
		{
			return _messages.Remove(id, out message);
		}
	}
}