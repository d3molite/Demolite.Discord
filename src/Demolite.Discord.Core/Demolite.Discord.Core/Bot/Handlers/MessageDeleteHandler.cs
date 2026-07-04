using Demolite.Discord.Core.Interfaces;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class MessageDeleteHandler(ILoggingService loggingService) : IMessageDeleteGatewayHandler
{
	public async ValueTask HandleAsync(MessageDeleteEventArgs arg)
	{
		if (arg.GuildId is null)
			return;
		
		await loggingService.LogMessageDeleted(arg.GuildId.Value, arg.ChannelId, arg.MessageId);
	}
}