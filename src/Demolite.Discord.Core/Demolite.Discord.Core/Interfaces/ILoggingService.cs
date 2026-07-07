using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Interfaces;

public interface ILoggingService
{
	public Task LogDefault(ulong guildId, EmbedProperties[] embed);

	public Task LogCritical(ulong guildId, EmbedProperties[] embed);
	
	public Task LogMessageDeleted(ulong guildId, ulong channelId, ulong messageId);

	public Task LogMessageUpdated(Message editedMessage);
	
	public Task LogUserBanned(ulong guildId, User user, RestGuild? syncGuild = null);
	
	public Task LogUserUnbanned(ulong guildId, User user, RestGuild? syncGuild = null);
}