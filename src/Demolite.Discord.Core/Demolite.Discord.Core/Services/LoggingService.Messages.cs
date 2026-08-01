using System.Text;
using System.Text.Json.Nodes;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;

namespace Demolite.Discord.Core.Services;

public partial class LoggingService
{
	public async Task LogMessageDeleted(ulong guildId, ulong channelId, ulong messageId)
	{
		var culture = GetLoggingCulture(guildId);

		var messageDeleted = Resources.GetResource(_ => LoggingResource.Body_MessageDeleted_NotFound)
			.Format(channelId);

		var messageInfo = $"A message was deleted in channel <#{channelId}>.";

		if (cache.TryRemove(channelId, messageId, out var message))
		{
			messageDeleted = message.Content;

			messageInfo = Resources.GetResource(_ => LoggingResource.Body_MessageDeleted, culture)
				.Format(message.Author.EmbedUser(), channelId);
		}

		List<JsonEmbedField> fields =
		[
			new()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_MessageDeleted, culture),
				Value = messageInfo,
				Inline = false
			},
			new()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_Content, culture),
				Value = messageDeleted,
				Inline = false
			}
		];

		if (message?.Attachments.Count > 0)
		{
			fields.Add(new JsonEmbedField
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_Attachments, culture),
				Value = string.Join(", ", message.Attachments.Select(attachment => attachment.FileName)),
				Inline = false
			});
		}

		await LogCritical(guildId, [fields.ToArray().CreateLogEmbed()]);
	}

	public async Task LogMessagesDeleted(ulong guildId, ulong channelId, IReadOnlyList<ulong> messageIds)
	{
		var culture = GetLoggingCulture(guildId);

		var messages = new List<Message>();
		var notFoundCount = 0;

		foreach (var messageId in messageIds)
		{
			if (cache.TryRemove(channelId, messageId, out var message))
				messages.Add(message);
			else
				notFoundCount++;
		}

		var authors = messages.Select(m => m.Author.EmbedUser()).Distinct().ToList();

		var messageInfo = Resources.GetResource(_ => LoggingResource.Body_MessagesDeletedBulk, culture)
			.Format(messages.Count, string.Join(", ", authors), channelId);

		List<JsonEmbedField> fields =
		[
			new()
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_MessageDeleted, culture),
				Value = messageInfo,
				Inline = false
			}
		];

		if (notFoundCount > 0)
		{
			fields.Add(new JsonEmbedField
			{
				Name = Resources.GetResource(_ => LoggingResource.Header_MessageDeleted, culture),
				Value = Resources.GetResource(_ => LoggingResource.Body_MessagesDeletedBulk_NotFound, culture)
					.Format(notFoundCount, channelId),
				Inline = false
			});
		}

		var lines = messages.Select(message =>
		{
			var line = message.Content;

			if (message.Attachments.Count > 0)
				line += $" ({string.Join(", ", message.Attachments.Select(a => a.FileName))})";

			return line;
		}).ToList();

		fields.AddRange(ChunkLines(lines).Select(chunk => new JsonEmbedField
		{
			Name = Resources.GetResource(_ => LoggingResource.Header_Content, culture),
			Value = chunk,
			Inline = false
		}));

		await LogCritical(guildId, [fields.ToArray().CreateLogEmbed()]);
	}

	private static IEnumerable<string> ChunkLines(IReadOnlyList<string> lines, int maxLength = 1024)
	{
		var current = new StringBuilder();

		foreach (var line in lines)
		{
			if (current.Length + line.Length + Environment.NewLine.Length > maxLength)
			{
				yield return current.ToString();
				current.Clear();
			}

			current.AppendLine(line);
		}

		if (current.Length > 0)
			yield return current.ToString();
	}

	public async Task LogMessageUpdated(Message editedMessage)
	{
		if (editedMessage.Author.Id == gatewayClient.Id)
			return;

		if (!cache.TryGet(editedMessage.ChannelId, editedMessage.Id, out var originalMessage))
		{
			await LogCritical(editedMessage.GuildId!.Value, [MessageEditedEmbed(editedMessage)]);
			return;
		}

		if (originalMessage.Content == editedMessage.Content)
			return;

		await LogCritical(editedMessage.GuildId!.Value, [MessageEditedEmbed(editedMessage, originalMessage).CreateLogEmbed()]);
	}

	private EmbedProperties MessageEditedEmbed(Message editedMessage, Message? originalMessage = null)
	{
		var locale = GetLoggingCulture(editedMessage.GuildId!.Value);
		List<JsonEmbedField> fields = [];

		var headerOriginalMessage = Resources.GetResource(
			_ => LoggingResource.Header_MessageEdited_OriginalMessage,
			locale
		);

		var headerNewMessage = Resources.GetResource(_ => LoggingResource.Header_MessageEdited_NewMessage, locale);

		fields.AddField(
			Resources.GetResource(_ => LoggingResource.Header_MessageEdited, locale),
			Resources.GetResource(_ => LoggingResource.Body_MessageEdited, locale)
				.Format(editedMessage.Author.EmbedUser())
		);

		if (originalMessage != null)
		{
			fields.AddField(headerOriginalMessage, originalMessage.Content);
			fields.AddField(headerNewMessage, GetChangesForNewMessage(originalMessage.Content, editedMessage.Content));
		}
		else
		{
			fields.AddField(
				headerOriginalMessage,
				Resources.GetResource(_ => LoggingResource.Body_MessageEdited_NotFound, locale)
			);

			fields.AddField(
				headerNewMessage,
				editedMessage.Content ?? Resources.GetResource(_ => LoggingResource.Body_MessageEdited_NotFound, locale)
			);
		}
		
		fields.AddField(
			Resources.GetResource(_ => LoggingResource.Header_Actions, locale), 
			$"[{Resources.GetResource(_ => LoggingResource.Body_Actions_LinkToMessage, locale)}]({editedMessage.ToMessageLink()})");

		return fields.ToArray()
			.CreateLogEmbed();
	}

	private static string GetChangesForNewMessage(string original, string modified)
	{
		var originalWords = original.Split(' ')
			.ToList();

		var modifiedWords = modified.Split(' ')
			.ToList();

		var newWords = new List<string>();

		foreach (var item in modifiedWords.Select((x, i) => new
					{
						Word = x,
						Index = i
					}
				))
		{
			var word = item.Word;
			var index = item.Index;

			if (originalWords.Contains(word))
			{
				newWords.Add(originalWords.IndexOf(word) == index ? word : $"**{word}**");
			}
			else
			{
				newWords.Add($"**{word}**");
			}
		}

		var newString = string.Join(" ", newWords);
		return newString;
	}
}