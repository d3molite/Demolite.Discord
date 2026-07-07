using NetCord;
using NetCord.Rest;

namespace Demolite.Discord.Core.Services;

public partial class LoggingService
{
	public Task LogUserBanned(ulong guildId, User user, RestGuild? syncGuild = null)
	{
		throw new NotImplementedException();
	}

	public Task LogUserUnbanned(ulong guildId, User user, RestGuild? syncGuild = null)
	{
		throw new NotImplementedException();
	}
}