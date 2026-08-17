using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Resources;
using NetCord;
using NetCord.Rest;

namespace Demolite.Discord.Core.Helpers.Voice;

public class VoiceChannelRenameMessageBuilder
{
    public const string ButtonId = "voice-rename";
    public const string ModalId = "voice-rename-modal";
    public const string InputId = "name";
    
    public static InteractionCallbackProperties NotInVoice(string? culture)
        => Ephemeral(MessageResources.ResourceManager.GetResource(_ => MessageResources.Body_VoiceRename_NotInVoice, culture));

    public static InteractionCallbackProperties Success(string? culture)
        => Ephemeral(MessageResources.ResourceManager.GetResource(_ => MessageResources.Body_VoiceRename_Success, culture));

    private static InteractionCallbackProperties Ephemeral(string content)
        => InteractionCallback.Message(new InteractionMessageProperties { Content = content, Flags = MessageFlags.Ephemeral });

    public static MessageProperties Create(string? culture) => new()
    {
        Content = MessageResources.ResourceManager.GetResource(_ => MessageResources.Body_VoiceRename_Message, culture),
        Components =
        [
            new ActionRowProperties(
            [
                new ButtonProperties(
                    ButtonId,
                    MessageResources.ResourceManager.GetResource(_ => MessageResources.Header_VoiceRename_Button, culture),
                    ButtonStyle.Primary)
            ])
        ]
    };
    
    public static ModalProperties CreateModal(string? culture, string currentName) => new(
        ModalId,
        MessageResources.ResourceManager.GetResource(_ => MessageResources.Header_VoiceRename_Modal, culture),
        [
            new LabelProperties(
                MessageResources.ResourceManager.GetResource(_ => MessageResources.Header_VoiceRename_Input, culture),
                new TextInputProperties(InputId, TextInputStyle.Short)
                {
                    Value = currentName,
                    MaxLength = 90
                })
        ]);
}