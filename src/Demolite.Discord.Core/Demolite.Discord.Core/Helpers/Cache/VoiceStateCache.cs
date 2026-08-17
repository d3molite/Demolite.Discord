using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using NetCord.Gateway;

namespace Demolite.Discord.Core.Helpers.Cache;

public sealed class VoiceStateCache
{
	private readonly ConcurrentDictionary<ulong, VoiceState> _users = new();

	public VoiceState? Update(VoiceState voiceState)
	{
		_users.TryGetValue(voiceState.UserId, out var previous);

		if (voiceState.ChannelId is null)
			_users.TryRemove(voiceState.UserId, out _);
		else
			_users[voiceState.UserId] = voiceState;

		return previous;
	}

	public bool TryGet(ulong userId, [NotNullWhen(true)] out VoiceState? voiceState)
		=> _users.TryGetValue(userId, out voiceState);
	
	public bool IsIn(ulong userId, ulong channelId)
		=> TryGet(userId, out var state) && state.ChannelId == channelId;
	
	public int CountIn(ulong channelId) => _users.Values.Count(v => v.ChannelId == channelId);
}