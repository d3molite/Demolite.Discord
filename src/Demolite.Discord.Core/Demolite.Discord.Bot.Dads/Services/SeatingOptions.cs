namespace Demolite.Discord.Bot.Dads.Services;

public sealed class SeatingOptions
{
    public const string SectionName = "Seating";

    public ulong GuildId { get; init; }
    public ulong CandidateRoleId { get; init; }
    public ulong MemberRoleId { get; init; }
    public ulong DoubleSeatRoleId { get; init; }
}