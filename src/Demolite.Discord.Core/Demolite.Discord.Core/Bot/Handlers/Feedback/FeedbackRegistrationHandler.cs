using Demolite.Discord.Core.Configuration;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Demolite.Discord.Core.Bot.Handlers.Feedback;

public class FeedbackRegistrationHandler(
    GatewayClient client,
    Dictionary<ulong, GuildConfig> guildConfigs) : IGuildCreateGatewayHandler
{
    public async ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        if (!guildConfigs.TryGetValue(arg.GuildId, out var config))
            return;

        if (config.ToolsConfig?.FeedbackChannelId is null)
            await UnregisterAsync(config.Id);
        else
            await client.Rest.CreateGuildApplicationCommandAsync(
                client.Id, config.Id, FeedbackCommand.CreateProperties(config.LoggingCulture));
    }

    private async Task UnregisterAsync(ulong guildId)
    {
        var commands = await client.Rest.GetGuildApplicationCommandsAsync(client.Id, guildId);
        var feedbackCommand = commands.FirstOrDefault(c => c.Name == FeedbackCommand.Name);
        if (feedbackCommand is not null)
            await client.Rest.DeleteGuildApplicationCommandAsync(client.Id, guildId, feedbackCommand.Id);
    }
}