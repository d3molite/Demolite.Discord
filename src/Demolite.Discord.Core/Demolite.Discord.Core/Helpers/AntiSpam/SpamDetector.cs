using NetCord.Gateway;

namespace Demolite.Discord.Core.Helpers.AntiSpam;

public static class SpamDetector
{
	private const int MaxIdenticalMessages = 5;

	/// <summary>
	/// Checks whether the messages contain too many identical texts, attachments or stickers.
	/// </summary>
	public static bool IsSpam(IReadOnlyCollection<Message> messages)
		=> HasRepeatedContent(messages)
			|| HasRepeatedAttachments(messages)
			|| HasRepeatedStickers(messages);

	private static bool HasRepeatedContent(IEnumerable<Message> messages)
	{
		var withContent = messages.Where(x => !string.IsNullOrEmpty(x.Content));

		return HasDuplicates(withContent, x => x.Content);
	}

	private static bool HasRepeatedAttachments(IEnumerable<Message> messages)
	{
		var withAttachments = messages.Where(x => x.Attachments.Count > 0);

		return HasDuplicates(withAttachments, GetAttachmentKey);
	}

	private static bool HasRepeatedStickers(IEnumerable<Message> messages)
	{
		var withStickers = messages.Where(x => x.Stickers.Count > 0);

		return HasDuplicates(withStickers, x => x.Stickers[0].Id);
	}

	private static string GetAttachmentKey(Message message)
		=> string.Join("", message.Attachments.Select(x => x.FileName));

	private static bool HasDuplicates<TKey>(IEnumerable<Message> messages, Func<Message, TKey> keySelector)
		=> messages.GroupBy(keySelector).Any(group => group.Count() > MaxIdenticalMessages);
}
