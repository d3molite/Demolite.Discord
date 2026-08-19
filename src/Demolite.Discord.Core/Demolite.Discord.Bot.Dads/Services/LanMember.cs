namespace Demolite.Discord.Bot.Dads.Services;

public class LanMember
{
    public ulong Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string UsernameForDisplay => Username.Split("|")[0];

    public string? PersonalName { get; set; }

    public bool IsDoubleSeat { get; set; }
    
    public string Group { get; set; } = string.Empty;
    
    public string Seat { get; set; } = string.Empty;
}