using Demolite.Discord.Core.Configuration;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using Serilog;

namespace Demolite.Discord.Core.Bot.Handlers;

public class NicknameChangeHandler(RestClient client, GuildConfig[] guildConfigs) : IReadyGatewayHandler
{
	public async ValueTask HandleAsync(ReadyEventArgs arg)
	{
		foreach (var config in guildConfigs)
		{
			if (config.CustomNickname is null)
				continue;

			try
			{
				var guild = await client.GetGuildAsync(config.Id);

				if (guild is null)
					continue;
				
				Log.Information("Setting nickname {Nickname} for guild {GuildId}", config.CustomNickname, guild.Name);

				await guild.ModifyCurrentUserAsync(properties => { properties.Nickname = config.CustomNickname; });
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error setting Nickname in guild {GuildId}", config.Id);
			}
		}
	}
}