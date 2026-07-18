using Demolite.Discord.Core.Interfaces;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class UserLeaveHandler(ILoggingService loggingService) : IGuildUserRemoveGatewayHandler
{
	public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg)
	{
		await loggingService.LogUserLeft(arg.GuildId, arg.User);
	}
}