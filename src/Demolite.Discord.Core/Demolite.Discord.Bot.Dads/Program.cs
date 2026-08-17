using Demolite.Discord.Bot.Dads.Components;
using Demolite.Discord.Core.Configuration;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.GetAndRegisterConfigs();

var discordConfig = builder.GetDiscordConfig();

builder.Services.AddDiscordGateway(options 
        =>
    {
        options.Intents = GatewayIntents.GuildMessages | GatewayIntents.MessageContent;
        options.Presence = discordConfig.CreatePresence();
    }
);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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