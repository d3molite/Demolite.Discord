using Demolite.Discord.Core.Interfaces;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class MessageDeleteBulkHandler(ILoggingService loggingService) : IMessageDeleteBulkGatewayHandler
{
	public async ValueTask HandleAsync(MessageDeleteBulkEventArgs arg)
	{
		if (arg.GuildId is null)
			return;
		
		await loggingService.LogMessagesDeleted(arg.GuildId.Value, arg.ChannelId, arg.MessageIds);
	}
}