using NetCord.JsonModels;
using NetCord.Rest;

namespace Demolite.Discord.Core.Extensions;

public static class EmbedExtensions
{
	public static MessageProperties CreateLogMessage(this EmbedProperties[] embed)
	{
		var processedEmbeds = embed.Select(emb => emb.ProcessEmbed()).ToArray();

		return new MessageProperties()
		{
			Content = "",
			Embeds = processedEmbeds
		};
	}

	private static EmbedProperties ProcessEmbed(this EmbedProperties emb)
	{
		if (emb.Fields is null)
			return emb;

		var embedFields = new List<EmbedFieldProperties>();

		foreach (var embField in emb.Fields)
		{
			if (embField.Value?.Length > 1024)
			{
				embedFields.AddRange(
					SplitInChunks(embField.Value, 1024)
						.Select(chunk => new EmbedFieldProperties()
							{
								Name = embField.Name,
								Value = chunk,
								Inline = embField.Inline
							}
						)
				);
			}
			else
			{
				embedFields.Add(embField);
			}
		}

		emb.Fields = embedFields.ToArray();
		return emb;
	}
	
	public static EmbedProperties CreateLogEmbed(this JsonEmbedField[] fields)
	{
		return new EmbedProperties()
		{
			Title = $"Log - {DateTime.Now:HH:mm:ss} (UTC)",
			Fields = fields.Select(field => field.CreateEmbedField())
				.ToArray()
		};
	}

	public static EmbedProperties CreateLogEmbed(this EmbedProperties field)
	{
		field.Title = $"Log - {DateTime.Now:HH:mm:ss} (UTC)";
		return field;
	}
	
	private static EmbedFieldProperties CreateEmbedField(this JsonEmbedField field)
	{
		return new EmbedFieldProperties()
		{
			Name = field.Name,
			Value = field.Value,
			Inline = field.Inline
		};
	}
	
	private static IEnumerable<string> SplitInChunks(string? str, int chunkSize)
	{
		if (string.IsNullOrEmpty(str))
			yield break;

		for (int i = 0; i < str.Length; i += chunkSize)
			yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
	}
}