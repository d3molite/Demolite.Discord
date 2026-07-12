using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Rest;

namespace Demolite.Discord.Core.Services;

public partial class LoggingService
{
	public async Task LogUserBanned(ulong guildId, User user, RestGuild? syncGuild = null)
	{
		var culture = GetLoggingCulture(guildId);
		await LogCritical(guildId, [UserBannedEmbed(culture, user, syncGuild).CreateLogEmbed()]);
	}

	private EmbedProperties UserBannedEmbed(string? culture, User user, RestGuild? syncGuild = null)
	{
		List<EmbedFieldProperties> fields =
		[
			new()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_UserBanned, culture),
				Value = Resources.GetResource(_ => LoggingResource.Body_UserBanned, culture)
					.Format(user.EmbedUser()),
				Inline = false
			}
		];
		
		if (syncGuild != null)
			fields.Add(new EmbedFieldProperties()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_UserBanned_Sync, culture),
				Value = Resources.GetResource(_ => LoggingResource.Body_UserBanned_Sync, culture).Format(syncGuild.Name),
				Inline = false,
			});
		
		return new EmbedProperties
		{
			Fields = fields.ToArray(),
			Color = new Color(0xFF0000)
		};
	}
	

	public async Task LogUserUnbanned(ulong guildId, User user, RestGuild? syncGuild = null)
	{
		var culture = GetLoggingCulture(guildId);
		await LogCritical(guildId, [UserUnbannedEmbed(culture, user, syncGuild).CreateLogEmbed()]);
	}
	
	private EmbedProperties UserUnbannedEmbed(string? culture, User user, RestGuild? syncGuild = null)
	{
		List<EmbedFieldProperties> fields =
		[
			new()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_UserUnbanned, culture),
				Value = Resources.GetResource(_ => LoggingResource.Body_UserUnbanned, culture)
					.Format(user.EmbedUser()),
				Inline = false
			}
		];
		
		if (syncGuild != null)
			fields.Add(new EmbedFieldProperties()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_UserBanned_Sync, culture),
				Value = Resources.GetResource(_ => LoggingResource.Body_UserBanned_Sync, culture).Format(syncGuild.Name),
				Inline = false
			});
		
		return new EmbedProperties
		{
			Fields = fields.ToArray(),
			Color = new Color(0xFF0000)
		};
	}
	
	public Task LogUserTimedOut(ulong guildId, User user)
	{
		var culture = GetLoggingCulture(guildId);
		return LogCritical(guildId, [UserTimedOutEmbed(culture, user).CreateLogEmbed()]);
	}

	private EmbedProperties UserTimedOutEmbed(string? culture, User user)
	{
		return new EmbedProperties()
		{
			Fields =
			[
				new()
				{
					Name = Resources.GetResource(_ => LoggingResource.Header_UserTimedOut, culture),
					Value = Resources.GetResource(_ => LoggingResource.Body_UserTimedOut, culture)
						.Format(user.EmbedUser()),
					Inline = false
				}
			],
			Color = new Color(0xFF0000)
		};
	}

	public Task LogUserJoined(ulong guildId, User user)
	{
		var culture = GetLoggingCulture(guildId);
		return LogDefault(guildId, [UserJoinedEmbed(culture, user).CreateLogEmbed()]);
	}
	
	private EmbedProperties UserJoinedEmbed(string? culture, User user)
	{
		return new EmbedProperties()
		{
			Fields =
			[
				new EmbedFieldProperties()
				{
					Name = Resources.GetResource(_ => LoggingResource.Header_UserJoined, culture),
					Value = Resources.GetResource(_ => LoggingResource.Body_UserJoined, culture)
						.Format(user.EmbedUser()),
					Inline = false
				},
				new EmbedFieldProperties()
				{
					Name = Resources.GetResource(_ => LoggingResource.Header_UserJoined_Age, culture),
					Value = user.CreatedAt.ToHumanFriendlyString(culture),
					Inline = false
				}
			]
		};
	}

	public Task LogUserLeft(ulong guildId, User user)
	{
		var culture = GetLoggingCulture(guildId);
		return LogDefault(guildId, [UserLeftEmbed(culture, user).CreateLogEmbed()]);
	}

	

	private EmbedProperties UserLeftEmbed(string? culture, User user)
	{
		return new EmbedProperties()
		{
			Fields =
			[
				new EmbedFieldProperties()
				{
					Name = Resources.GetResource(_ => LoggingResource.Header_UserLeft, culture),
					Value = Resources.GetResource(_ => LoggingResource.Body_UserLeft, culture)
						.Format(user.EmbedUser()),
					Inline = false
				}
			]
		};
	}
}