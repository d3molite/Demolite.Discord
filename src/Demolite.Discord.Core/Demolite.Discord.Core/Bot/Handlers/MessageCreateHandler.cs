using Demolite.Discord.Core.Helpers.Cache;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers;

public class MessageCreateHandler(ChannelMessageCache cache) : IMessageCreateGatewayHandler
{
	public ValueTask HandleAsync(Message arg)
	{
		cache.Add(arg);
		return ValueTask.CompletedTask;
	}
}