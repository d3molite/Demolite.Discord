using Demolite.Discord.Bot.Dads.Components;
using Demolite.Discord.Bot.Dads.Services;
using Demolite.Discord.Core.Configuration;
using LiteDB;
using MudBlazor.Services;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.GetAndRegisterConfigs();

builder.Services.Configure<SeatingOptions>(
    builder.Configuration.GetSection(SeatingOptions.SectionName));

var discordConfig = builder.GetDiscordConfig();

builder.Services.AddDiscordGateway(options 
        =>
    {
        options.Intents = GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.AllNonPrivileged | GatewayIntents.GuildUsers;
        options.Presence = discordConfig.CreatePresence();
    }
);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddSingleton<LiteDatabase>(_ =>
{
    var dbDirectory = Path.Combine("db");
    
    if (!Directory.Exists(dbDirectory))
        Directory.CreateDirectory(dbDirectory);

    var dbPath = Path.Combine(dbDirectory, "lan.db");
    return new LiteDatabase($"Filename={dbPath}");
});

builder.Services.AddSingleton<LanMemberHandler>();
builder.Services.AddSingleton<SeatingConfigurationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();