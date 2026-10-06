using Demolite.Discord.Core.Interfaces;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class MessageEditHandler(ILoggingService loggingService) : IMessageUpdateGatewayHandler
{
	public async ValueTask HandleAsync(Message arg)
	{
		if (arg.GuildId is null || string.IsNullOrWhiteSpace(arg.Content))
			return;
		
		await loggingService.LogMessageUpdated(arg);
	}
}