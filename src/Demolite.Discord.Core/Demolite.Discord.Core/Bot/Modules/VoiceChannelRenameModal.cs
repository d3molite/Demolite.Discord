using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Helpers.Voice;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Demolite.Discord.Core.Bot.Modules;

public sealed class VoiceChannelRenameModal(
    Dictionary<ulong, GuildConfig> guildConfigs,
    VoiceChannelRenameAccessChecker access,
    VoiceChannelCache createdChannels,
    RestClient client) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction(VoiceChannelRenameMessageBuilder.ModalId)]
    public async Task RenameAsync()
    {
        if (Context.Interaction.GuildId is not { } guildId)
            return;

        var culture = guildConfigs.GetLoggingCulture(guildId);
        var channelId = Context.Channel.Id;

        if (!access.CanRename(guildId, channelId, Context.User.Id)
            || !createdChannels.TryGetNumber(channelId, out var number))
        {
            await Context.Interaction.SendResponseAsync(VoiceChannelRenameMessageBuilder.NotInVoice(culture));
            return;
        }

        await client.ModifyGuildChannelAsync(channelId,
            options => options.Name = VoiceChannelName.Format(number, ReadEnteredName()));

        await Context.Interaction.SendResponseAsync(VoiceChannelRenameMessageBuilder.Success(culture));
    }

    private string ReadEnteredName()
        => Context.Components
            .OfType<Label>()
            .Select(label => label.Component)
            .OfType<TextInput>()
            .Single(input => input.CustomId == VoiceChannelRenameMessageBuilder.InputId)
            .Value.Trim();
}