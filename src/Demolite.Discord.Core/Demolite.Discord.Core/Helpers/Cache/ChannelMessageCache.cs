using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using NetCord.Gateway;

namespace Demolite.Discord.Core.Helpers.Cache;

public sealed class ChannelMessageCache(int capacityPerChannel = 300)
{
	private readonly ConcurrentDictionary<ulong, MessageCache> _channels = new();

	public void Add(Message message)
	{
		var cache = _channels.GetOrAdd(
			message.ChannelId,
			_ => new MessageCache(capacityPerChannel));

		cache.Add(message);
	}

	public bool TryGet(ulong channelId, ulong messageId, [NotNullWhen(true)] out Message? message)
	{
		message = null;
		return _channels.TryGetValue(channelId, out var cache)
				&& cache.TryGet(messageId, out message);
	}

	public bool TryRemove(ulong channelId, ulong messageId, [NotNullWhen(true)] out Message? message)
	{
		message = null;
		return _channels.TryGetValue(channelId, out var cache)
				&& cache.TryRemove(messageId, out message);
	}
}