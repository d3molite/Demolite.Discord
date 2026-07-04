using Demolite.Discord.Core.Bot.Handlers;
using Demolite.Discord.Core.Configuration;
using Demolite.Discord.Core.Helpers.Cache;
using Demolite.Discord.Core.Interfaces;
using Demolite.Discord.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.GetAndRegisterConfigs();

var discordConfig = builder.GetDiscordConfig();

builder.Services.AddDiscordGateway(options 
	=>
	{
		options.Intents = GatewayIntents.GuildMessages | GatewayIntents.MessageContent;
		options.Presence = discordConfig.CreatePresence();
	}
);

builder.Services.AddSingleton<ChannelMessageCache>();
builder.Services.AddGatewayHandler<MessageCreateHandler>();
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddGatewayHandler<AntiSpamHandler>();
builder.Services.AddGatewayHandler<MessageDeleteHandler>();

var host = builder.Build();


await host.RunAsync();