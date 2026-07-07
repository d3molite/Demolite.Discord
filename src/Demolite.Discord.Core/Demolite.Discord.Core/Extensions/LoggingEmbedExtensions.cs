using System.Text;
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