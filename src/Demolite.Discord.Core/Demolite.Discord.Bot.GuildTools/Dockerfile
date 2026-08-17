FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Demolite.Discord.Bot.GuildTools/Demolite.Discord.Bot.GuildTools.csproj", "Demolite.Discord.Bot.GuildTools/"]
COPY ["Demolite.Discord.Core/Demolite.Discord.Core.csproj", "Demolite.Discord.Core/"]
RUN dotnet restore "Demolite.Discord.Bot.GuildTools/Demolite.Discord.Bot.GuildTools.csproj"
COPY . .
WORKDIR "/src/Demolite.Discord.Bot.GuildTools"
RUN dotnet build "./Demolite.Discord.Bot.GuildTools.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Demolite.Discord.Bot.GuildTools.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demolite.Discord.Bot.GuildTools.dll"]
