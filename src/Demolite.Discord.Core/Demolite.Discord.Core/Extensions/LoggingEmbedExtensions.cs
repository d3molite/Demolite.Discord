using System.Text;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;

namespace Demolite.Discord.Core.Extensions;

public static class LoggingEmbedExtensions
{
	public static void AddField(this List<JsonEmbedField> fields, string title, string content, bool inline = false)
	{
		fields.Add(
			new JsonEmbedField()
			{
				Name = title,
				Value = content,
				Inline = inline
			}
		);
	}
	
	public static string ToMessageLink(this Message message)
		=> $"https://discord.com/channels/{message.GuildId}/{message.ChannelId}/{message.Id}";

	public static string EmbedUser(this Message message)
	{
		return EmbedUser(message.Author);
	}

	public static string ToHumanFriendlyString(this DateTimeOffset time, string? locale)
	{
		var resources = UnitResource.ResourceManager;
		var timeSpan =  time - DateTimeOffset.Now;
		
		var roundedAge = Math.Round(timeSpan.TotalDays, 2);

		return roundedAge switch
		{
			// if the time is smaller than one day.
			< 1 => $"{Math.Round(timeSpan.TotalHours, 2)} {resources.GetResource(x => UnitResource.Hours, locale)}",

			// if the time is smaller than a year.
			< 365 => $"{roundedAge} {resources.GetResource(x => UnitResource.Days, locale)}",

			// if the time is greater than a year.
			var _ => $"{Math.Round(timeSpan.TotalDays / 365, 2)} {resources.GetResource(x => UnitResource.Years, locale)}",
		};
	}

	public static string EmbedUser(this User user)
	{
		var sb = new StringBuilder();

		if (user is GuildUser guildUser)
		{
			sb.Append(guildUser.Nickname ?? guildUser.Username);
		}
		else
		{
			sb.Append(user.Username);
		}

		sb.Append($" (<@{user.Id}>)");
		
		return sb.ToString();
	}
}