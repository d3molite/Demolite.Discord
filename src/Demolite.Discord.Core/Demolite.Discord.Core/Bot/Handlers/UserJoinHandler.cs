using Demolite.Discord.Core.Interfaces;
using NetCord;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class UserJoinHandler(ILoggingService loggingService) : IGuildUserAddGatewayHandler
{
	public async ValueTask HandleAsync(GuildUser arg)
	{
		await loggingService.LogUserJoined(arg.GuildId, arg);
	}
}