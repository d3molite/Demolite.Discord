using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Interfaces;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers;

public class UserBanRemoveHandler(ILoggingService loggingService, GuildConfig[] configs, RestClient client) : IGuildBanRemoveGatewayHandler
{
	private readonly Dictionary<ulong, RestGuild> _userUnbanInProgress = [];

	public async ValueTask HandleAsync(GuildBanEventArgs arg)
	{
		var baseGuild = await client.GetGuildAsync(arg.GuildId);

		if (!_userUnbanInProgress.TryAdd(arg.User.Id, baseGuild))
		{
			await loggingService.LogUserUnbanned(arg.GuildId, arg.User, baseGuild);
			return;
		}

		await loggingService.LogUserUnbanned(arg.GuildId, arg.User);

		var user = arg.User;

		foreach (var config in configs.Where(x => x.Id != arg.GuildId))
		{
			try
			{
				var guild = await client.GetGuildAsync(config.Id);

				await guild.UnbanUserAsync(
					user.Id,
					new RestRequestProperties()
					{
						AuditLogReason = $"Unban Sync from {baseGuild.Name}"
					}
				);
			}
			catch (Exception ex)
			{
				await loggingService.LogCritical(
					arg.GuildId,
					[
						new EmbedProperties()
						{
							Title = "Unban Sync Error",
							Description =
								$"An error occurred while syncing the unban for user <@{user.Id}> ({user.Username}#{user.Discriminator}) to guild <@{config.Id}> ({config.Name}).\n\nError: {ex.Message}",
							Color = new Color(0xFF0000)
						}
					]
				);
			}
		}

		_userUnbanInProgress.Remove(arg.User.Id);
	}
}