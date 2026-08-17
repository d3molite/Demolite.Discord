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
	
	public GuardConfig? GuardConfig { get; set; }
	
	public ToolsConfig? ToolsConfig { get; set; }
	
	public string? LoggingCulture { get; set; }
	
	public string? CustomNickname { get; set; }
}

[UsedImplicitly]
public class GuardConfig
{
	public required ulong LogDefault { get; set; }
	
	public required ulong LogCritical { get; set; }
	
	public ulong? HoneyPotChannelId { get; set; }

	public string[] AntispamExceptions { get; set; } = [];
}

[UsedImplicitly]
public class ToolsConfig
{
	public ulong? VoiceChannelCreateId { get; set; }
	public bool? VoiceChannelRenameAllowed { get; set; }

	public int VoiceChannelDeleteDelaySeconds { get; set; } = 60;
}