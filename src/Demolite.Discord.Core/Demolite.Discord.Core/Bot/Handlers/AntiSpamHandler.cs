using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Helpers.AntiSpam;
using Demolite.Discord.Core.Interfaces;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers;

public class AntiSpamHandler : IMessageCreateGatewayHandler
{
	private readonly GatewayClient _client;
	private readonly AntiSpamHelper _helper;
	private readonly PeriodicTimer _cleanupTimer = new(TimeSpan.FromMinutes(4));

	public AntiSpamHandler(
		RestClient restClient,
		GatewayClient client,
		GuildConfig[] guildConfigs,
		ILoggingService loggingService
	)
	{
		_client = client;
		_helper = new AntiSpamHelper(guildConfigs, new SpamPunisher(restClient, loggingService));

		_ = RunCleanupAsync();
	}

	public ValueTask HandleAsync(Message message)
	{
		if (message.Author.Id != _client.Id)
			_helper.CheckForSpam(message);

		return ValueTask.CompletedTask;
	}

	private async Task RunCleanupAsync()
	{
		while (await _cleanupTimer.WaitForNextTickAsync())
			_helper.RemoveExpiredMessages();
	}
}
