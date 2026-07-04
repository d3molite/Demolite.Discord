using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;

namespace Demolite.Discord.Core.Configuration;

public static class ConfigExtensions
{
	extension(IHostApplicationBuilder builder)
	{
		public void GetAndRegisterConfigs()
		{
			var config = builder.GetDiscordConfig();
			var guilds = builder.GetGuildConfigs();

			builder.Services.AddSingleton(config);
			builder.Services.AddSingleton(guilds);
		}

		public ConfigurationSettings GetDiscordConfig()
		{
			return builder.Configuration.GetSection("Configuration")
						.Get<ConfigurationSettings>() ??
					throw new InvalidOperationException("Discord configuration section is missing.");
		}

		private List<GuildConfig> GetGuildConfigs()
		{
			return builder.Configuration.GetSection("Guilds").Get<List<GuildConfig>>() 
					?? throw new InvalidOperationException("Guilds configuration is missing");
		}
	}

	extension(ConfigurationSettings settings)
	{
		public PresenceProperties CreatePresence()
			=> new(Enum.Parse<UserStatusType>(settings.Presence.StatusType, true))
			{
				Activities = settings.Presence.Activities.Select(x => x.GetFromSetting()).ToList()
			};
	}
	
	private static UserActivityProperties GetFromSetting(this ActivitySettings activitySettings)
		=> new(
			activitySettings.Name,
			Enum.Parse<UserActivityType>(activitySettings.Type, true)
		);
}