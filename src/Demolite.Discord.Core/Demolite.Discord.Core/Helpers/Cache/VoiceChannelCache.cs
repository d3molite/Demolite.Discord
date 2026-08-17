using System.Collections.Concurrent;

namespace Demolite.Discord.Core.Helpers.Cache;

public sealed class VoiceChannelCache
{
	private readonly ConcurrentDictionary<ulong, (ulong GuildId, int Number)> _channels = new();
	private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _pendingDeletes = new();

	public void Add(ulong channelId, ulong guildId, int number) => _channels[channelId] = (guildId, number);

	public bool Contains(ulong channelId) => _channels.ContainsKey(channelId);

	public int GetFreeNumber(ulong guildId)
	{
		var used = _channels.Values.Where(c => c.GuildId == guildId).Select(c => c.Number).ToHashSet();
		return Enumerable.Range(1, used.Count + 1).First(n => !used.Contains(n));
	}

	public CancellationToken StartPendingDelete(ulong channelId)
	{
		var cts = new CancellationTokenSource();
		_pendingDeletes.AddOrUpdate(channelId, cts, (_, existing) =>
		{
			existing.Cancel();
			existing.Dispose();
			return cts;
		});
		return cts.Token;
	}

	public void CancelPendingDelete(ulong channelId)
	{
		if (!_pendingDeletes.TryRemove(channelId, out var cts))
			return;

		cts.Cancel();
		cts.Dispose();
	}

	public void Remove(ulong channelId)
	{
		CancelPendingDelete(channelId);
		_channels.TryRemove(channelId, out _);
	}
	
	public bool TryGetNumber(ulong channelId, out int number)
	{
		var found = _channels.TryGetValue(channelId, out var entry);
		number = entry.Number;
		return found;
	}
}