using System.Resources;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers.Feedback;

public static class FeedbackCommand
{
    public const string Name = "feedback";
    public const string ModalId = "feedback-modal";

    private const string TextInputId = "message";
    private const int MaxChunkLength = 1000;
    private const int MaxMessageLength = 4000;

    private static ResourceManager Resources => MessageResources.ResourceManager;

    public static SlashCommandProperties CreateProperties(string? culture) =>
        new(Name, Resources.GetResource(_ => MessageResources.Body_FeedbackCommandDescription, culture));

    public static ModalProperties CreateModal(string? culture) =>
        new(
            ModalId,
            Resources.GetResource(_ => MessageResources.Header_FeedbackModal, culture),
            [
                new LabelProperties(
                    Resources.GetResource(_ => MessageResources.Body_FeedbackOptionDescription, culture),
                    new TextInputProperties(TextInputId, TextInputStyle.Paragraph)
                    {
                        Required = true,
                        MaxLength = MaxMessageLength
                    })
            ]);

    public static string ReadText(ModalInteraction modal) =>
        modal.Data.Components
            .OfType<Label>()
            .Select(label => label.Component)
            .OfType<TextInput>()
            .Single(input => input.CustomId == TextInputId)
            .Value;

    public static MessageProperties CreateMessage(ulong userId, string text, string? culture)
    {
        var chunks = SplitAtWordBorders(text, MaxChunkLength).ToList();

        return new MessageProperties
        {
            Content = Resources.GetResource(_ => MessageResources.Header_Feedback, culture)
                .Format($"<@{userId}>", DateTime.Now.ToString("dd.MM.yyyy")),
            Embeds = chunks.Select((chunk, i) => CreateChunkEmbed(chunk, i + 1, chunks.Count)).ToList(),
            AllowedMentions = AllowedMentionsProperties.None
        };
    }

    private static EmbedProperties CreateChunkEmbed(string chunk, int index, int total) => new()
    {
        Title = $"{index}/{total}",
        Description = chunk
    };

    private static IEnumerable<string> SplitAtWordBorders(string text, int maxLength)
    {
        var remaining = text.AsMemory();
        while (remaining.Length > maxLength)
        {
            var splitIndex = remaining.Span[..(maxLength + 1)].LastIndexOf(' ');
            if (splitIndex <= 0)
                splitIndex = maxLength;

            yield return remaining[..splitIndex].ToString();
            remaining = remaining[splitIndex..].TrimStart();
        }

        yield return remaining.ToString();
    }
}