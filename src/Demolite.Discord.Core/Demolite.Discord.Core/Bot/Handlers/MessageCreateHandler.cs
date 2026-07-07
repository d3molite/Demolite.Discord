using Demolite.Discord.Core.Helpers.Cache;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class MessageCreateHandler(ChannelMessageCache cache, GatewayClient client) : IMessageCreateGatewayHandler
{
	public ValueTask HandleAsync(Message arg)
	{
		if (arg.Author.Id == client.Id)
			return ValueTask.CompletedTask;
		
		cache.Add(arg);
		return ValueTask.CompletedTask;
	}
}