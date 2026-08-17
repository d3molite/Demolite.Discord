using Demolite.Discord.Core.Bot.Handlers;
using Demolite.Discord.Core.Bot.Handlers.Voice;
using Demolite.Discord.Core.Bot.Modules;
using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Extensions;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Helpers.Voice;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console()
	.CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.GetAndRegisterConfigs();
var discordConfig = builder.GetDiscordConfig();

builder.Services.AddDiscordGateway(options =>
	{
		options.Intents = GatewayIntents.AllNonPrivileged;
		options.Presence = discordConfig.CreatePresence();
	}
);

builder.Services.AddGatewayHandler<NicknameChangeHandler>();

builder.Services.AddComponentInteractions<ModalInteraction, ModalInteractionContext>();
builder.Services.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>();
builder.Services.RegisterVoiceServices();

var host = builder.Build();
host.AddModules(typeof(VoiceChannelRenameButtonModule).Assembly);

await host.RunAsync();