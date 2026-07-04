using Demolite.Discord.Core.Helpers.AntiSpam;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers;

public class AntiSpamHandler : IMessageCreateGatewayHandler
{
	private readonly RestClient _restClient;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(4));
	private readonly Dictionary<ulong, AntiSpamHelper> _guildHelpers = new();

	public AntiSpamHandler(ILogger<AntiSpamHandler> logger, RestClient restClient)
	{
		_restClient = restClient;
		Task.Run(async () => await Setup());
		Task.Run(async () => await CleanupQueues());
	}

	public async ValueTask HandleAsync(Message arg)
	{
		if (arg.GuildId is null)
			return;

		var helper = _guildHelpers[arg.GuildId.Value];
		await helper.CheckForSpam(arg);
	}

	private async Task Setup()
	{
		await foreach (var guild in _restClient.GetCurrentUserGuildsAsync())
		{
			_guildHelpers.Add(guild.Id, new AntiSpamHelper(_restClient, guild));
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