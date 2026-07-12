using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Helpers.AntiSpam;
using Demolite.Discord.Core.Interfaces;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers;

public class AntiSpamHandler : IMessageCreateGatewayHandler
{
	private readonly RestClient _restClient;
	private readonly GatewayClient _client;
	private readonly GuildConfig[] _guildConfigs;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(4));
	private readonly Dictionary<ulong, AntiSpamHelper> _guildHelpers = new();
	private readonly ILoggingService _loggingService;

	public AntiSpamHandler(
		ILogger<AntiSpamHandler> logger,
		RestClient restClient,
		GatewayClient client,
		GuildConfig[] guildConfigs,
		ILoggingService loggingService
	)
	{
		_restClient = restClient;
		_client = client;
		_guildConfigs = guildConfigs;
		_loggingService = loggingService;
		Task.Run(async () => await Setup());
		Task.Run(async () => await CleanupQueues());
	}

	public async ValueTask HandleAsync(Message arg)
	{
		if (arg.Author.Id == _client.Id)
			return;

		if (arg.GuildId is null)
			return;

		var helper = _guildHelpers[arg.GuildId.Value];
		await helper.CheckForSpam(arg);
	}

	private async Task Setup()
	{
		await foreach (var guild in _restClient.GetCurrentUserGuildsAsync())
		{
			_guildHelpers.Add(guild.Id, new AntiSpamHelper(_restClient, guild, _guildConfigs,  _loggingService));
		}
	}

	private async Task CleanupQueues()
	{
		while (await _timer.WaitForNextTickAsync())
		{
			foreach (var helper in _guildHelpers.Values)
			{
				helper.CleanupQueues();
			}
		}
	}
}