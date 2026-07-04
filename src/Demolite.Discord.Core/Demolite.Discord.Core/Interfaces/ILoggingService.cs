using NetCord;
using NetCord.Rest;

namespace Demolite.Discord.Core.Interfaces;

public interface ILoggingService
{
	public Task LogDefault(ulong guildId, EmbedProperties[] embed);

	public Task LogCritical(ulong guildId, EmbedProperties[] embed);
	
	public Task LogMessageDeleted(ulong guildId, ulong channelId, ulong messageId);
}