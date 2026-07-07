using JetBrains.Annotations;

namespace Demolite.Discord.Core.Configuration;

public class ConfigurationSettings
{
	public required PresenceSettings Presence { get; set; }
}

public class PresenceSettings
{
	public required string StatusType { get; set; }
	public required List<ActivitySettings> Activities { get; set; }
}

public class ActivitySettings
{
	public required string Name { get; set; }
	public required string Type { get; set; }
}

[UsedImplicitly]
public class GuildConfig
{
	public required ulong Id { get; set; }

	public required string Name { get; set; }
	
	public required ulong LogDefault { get; set; }
	
	public required ulong LogCritical { get; set; }
	
	public ulong? HoneyPotChannelId { get; set; }
	
	public string LoggingCulture { get; set; }
}