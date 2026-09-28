using System.Diagnostics.CodeAnalysis;
using System.Resources;
using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Demolite.Discord.Core.Bot.Handlers.Feedback;

public class FeedbackInteractionHandler(
    GatewayClient client,
    Dictionary<ulong, GuildConfig> guildConfigs) : IInteractionCreateGatewayHandler
{
    private static ResourceManager Resources => MessageResources.ResourceManager;

    public async ValueTask HandleAsync(Interaction interaction)
    {
        switch (interaction)
        {
            case SlashCommandInteraction { Data.Name: FeedbackCommand.Name } command:
                await ShowModalAsync(command);
                break;
            case ModalInteraction { Data.CustomId: FeedbackCommand.ModalId } modal:
                await ForwardFeedbackAsync(modal);
                break;
        }
    }

    private async Task ShowModalAsync(SlashCommandInteraction command)
    {
        if (!TryGetFeedbackChannel(command.GuildId, out var config, out _))
            return;

        await command.SendResponseAsync(InteractionCallback.Modal(FeedbackCommand.CreateModal(config.LoggingCulture)));
    }

    private async Task ForwardFeedbackAsync(ModalInteraction modal)
    {
        if (!TryGetFeedbackChannel(modal.GuildId, out var config, out var channelId))
            return;

        var message = FeedbackCommand.CreateMessage(modal.User.Id, FeedbackCommand.ReadText(modal), config.LoggingCulture);
        await client.Rest.SendMessageAsync(channelId, message);

        await modal.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties
        {
            Content = Resources.GetResource(_ => MessageResources.Body_FeedbackThanks, config.LoggingCulture),
            Flags = MessageFlags.Ephemeral
        }));
    }

    private bool TryGetFeedbackChannel(
        ulong? guildId,
        [NotNullWhen(true)] out GuildConfig? config,
        out ulong channelId)
    {
        config = null;
        channelId = 0;

        if (guildId is not { } id || !guildConfigs.TryGetValue(id, out config))
            return false;

        if (config.ToolsConfig?.FeedbackChannelId is not { } feedbackChannelId)
            return false;

        channelId = feedbackChannelId;
        return true;
    }
}