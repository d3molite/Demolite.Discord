using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Interfaces;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers;

public class UserBanAddHandler(ILoggingService loggingService, GuildConfig[] configs, RestClient client) : IGuildBanAddGatewayHandler
{
	private readonly Dictionary<ulong, RestGuild> _userBanInProgress = [];
	
	public async ValueTask HandleAsync(GuildBanEventArgs arg)
	{
		var baseGuild = await client.GetGuildAsync(arg.GuildId);

		if (!_userBanInProgress.TryAdd(arg.User.Id, baseGuild))
		{
			var syncGuild = _userBanInProgress[arg.User.Id];
			await loggingService.LogUserBanned(arg.GuildId, arg.User, syncGuild);
			return;
		}
		
		await loggingService.LogUserBanned(arg.GuildId, arg.User);

		var user = arg.User;

		foreach (var config in configs.Where(x => x.Id != arg.GuildId))
		{
			try
			{
				var guild = await client.GetGuildAsync(config.Id);

				await guild.BanUserAsync(
					user.Id,
					0,
					new RestRequestProperties()
					{
						AuditLogReason = $"Ban Sync from {baseGuild.Name}"
					}
				);
			}
			catch (Exception ex)
			{
				await loggingService.LogCritical(arg.GuildId,
				[
					new EmbedProperties()
					{
						Title = "Ban Sync Error",
						Description = $"An error occurred while syncing the ban for user <@{user.Id}> ({user.Username}#{user.Discriminator}) to guild <@{config.Id}> ({config.Name}).\n\nError: {ex.Message}",
						Color = new Color(0xFF0000) 
					}
				]
				);
			}
		}
		
		_userBanInProgress.Remove(arg.User.Id);
	}
}