using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Helpers.Voice;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Demolite.Discord.Core.Bot.Modules;

public sealed class VoiceChannelRenameButtonModule(
    Dictionary<ulong, GuildConfig> guildConfigs,
    VoiceChannelRenameAccessChecker access,
    RestClient client) :  ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(VoiceChannelRenameMessageBuilder.ButtonId)]
    public async Task OpenRenameModalAsync()
    {
        if (Context.Interaction.GuildId is not { } guildId)
            return;

        var culture = guildConfigs.GetLoggingCulture(guildId);
        var channelId = Context.Channel.Id;

        if (!access.CanRename(guildId, channelId, Context.User.Id))
        {
            await Context.Interaction.SendResponseAsync(VoiceChannelRenameMessageBuilder.NotInVoice(culture));
            return;
        }

        var channel = (VoiceGuildChannel)await client.GetChannelAsync(channelId);
        var currentName = VoiceChannelName.StripNumber(channel.Name);

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Modal(VoiceChannelRenameMessageBuilder.CreateModal(culture, currentName)));
    }
}