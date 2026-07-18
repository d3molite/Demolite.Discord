using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Interfaces;
using NetCord.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Services;

public class CustomNicknameManager : ICustomNicknameManager
{
	private readonly List<GuildConfig> _guildConfigs;
	private readonly RestClient _client;

	public CustomNicknameManager(List<GuildConfig> guildConfigs, RestClient client)
	{
		_guildConfigs = guildConfigs;
		_client = client;
		
		Task.Run(async() => await SetGuildNicknames());
	}

	private async Task SetGuildNicknames()
	{
		foreach (var config in _guildConfigs)
		{
			if (config.CustomNickname is null)
				continue;
			
			var guild = await _client.GetGuildAsync(config.Id);
			
			if (guild is null)
				continue;
			
			await guild.ModifyCurrentUserAsync(properties =>
			{
				properties.Nickname = config.CustomNickname;
			});
		}
	}
}