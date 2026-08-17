using Demolite.Discord.Core.Bot.Handlers.Voice;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Helpers.Voice;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Hosting.Gateway;
using VoiceChannelRecoveryHandler = Demolite.Discord.Core.Bot.Handlers.Voice.VoiceChannelRecoveryHandler;

namespace Demolite.Discord.Core.Extensions;

public static class HandlerExtensions
{
    public static void RegisterVoiceServices(this IServiceCollection collection)
    {
        collection.AddSingleton<VoiceStateCache>();
        collection.AddSingleton<VoiceChannelCache>();
        collection.AddSingleton<VoiceChannelCleaner>();
        collection.AddGatewayHandler<CustomVoiceChannelHandler>();
        collection.AddGatewayHandler<VoiceChannelRecoveryHandler>();
        collection.AddSingleton<VoiceChannelRenameAccessChecker>();
    }
}