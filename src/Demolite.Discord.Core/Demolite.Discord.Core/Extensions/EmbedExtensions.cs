using NetCord.JsonModels;
using NetCord.Rest;

namespace Demolite.Discord.Core.Extensions;

public static class EmbedExtensions
{
	public static MessageProperties CreateLogMessage(this EmbedProperties[] embed)
	{
		return new MessageProperties()
		{
			Content = "",
			Embeds = embed
		};
	}
	
	public static EmbedProperties CreateLogEmbed(this JsonEmbedField[] fields)
	{
		return new EmbedProperties()
		{
			Title = $"Log - {DateTime.Now:HH:mm:ss}",
			Fields = fields.Select(field => field.CreateEmbedField())
				.ToArray()
		};
	}

	public static EmbedProperties CreateLogEmbed(this EmbedProperties field)
	{
		field.Title = $"Log - {DateTime.Now:HH:mm:ss}";
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
}