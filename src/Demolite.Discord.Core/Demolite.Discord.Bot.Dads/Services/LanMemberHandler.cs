using System.Text.RegularExpressions;
using LiteDB;
using Microsoft.Extensions.Options;
using NetCord.Rest;
using Serilog;

namespace Demolite.Discord.Bot.Dads.Services;

public partial class LanMemberHandler
{
    private readonly RestClient _client;
    private readonly ILiteCollection<LanMember> _members;
    private readonly SeatingOptions _options;

    public LanMemberHandler(RestClient client, LiteDatabase db, IOptions<SeatingOptions> seatingOptions)
    {
        _client = client;
        _members = db.GetCollection<LanMember>("members");
        _options = seatingOptions.Value;
    }

    private ulong GuildId => _options.GuildId;
    private ulong CandidateRoleId => _options.CandidateRoleId;
    private ulong MemberRoleId => _options.MemberRoleId;
    private ulong DoubleSeatRoleId => _options.DoubleSeatRoleId;

    public List<LanMember> LanMembers { get; set; } = [];

    public void SaveLanMembers()
    {
        _members.Upsert(LanMembers);
        Log.Debug("Saved {Count} lan members", LanMembers.Count);
    }

    public void LoadLanMembersFromDatabase()
    {
        LanMembers = _members.FindAll().OrderBy(x => x.UsernameForDisplay).ToList();
        Log.Debug("Loaded {Count} lan members from database", LanMembers.Count);
    }

    public async Task UpdateLanMembersFromDiscord()
    {
        var existing = _members.FindAll().ToDictionary(m => m.Id);
        var current = new List<LanMember>();

        await foreach (var user in _client.GetGuildUsersAsync(GuildId))
        {
            if (!user.RoleIds.Contains(MemberRoleId))
                continue;

            existing.TryGetValue(user.Id, out var stored);

            current.Add(new LanMember
            {
                Id = user.Id,
                Username = CleanName(user.Nickname ?? user.GlobalName ?? user.Username, stored?.PersonalName),
                PersonalName = stored?.PersonalName ?? string.Empty,
                IsDoubleSeat = user.RoleIds.Contains(DoubleSeatRoleId),
                Group = stored?.Group ?? string.Empty,
                Seat = stored?.Seat ?? string.Empty
            });
        }

        var currentIds = current.Select(m => m.Id).ToHashSet();
        var removedIds = existing.Keys.Where(id => !currentIds.Contains(id)).ToList();

        foreach (var id in removedIds)
            _members.Delete(id);

        LanMembers = current;
        SaveLanMembers();

        Log.Debug("Updated {Count} lan members, removed {RemovedCount}", LanMembers.Count, removedIds.Count);
    }

    private string CleanName(string input, string? personalName)
    {
        var original = input;

        input = SeatRegex().Replace(input, string.Empty);
        input = input.Replace("DADS", "");
        input = input.Replace("XMG", "");
        input = input.Replace("|", " ");
        input = input.Replace("/", " ");

        var components = input.Split(" ");

        if (components.Length > 1)
            components = components.Where(x => !x.Equals(personalName, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

        var output = string.Join(" ", components);
        output = output.Trim();
        output = output.Trim('-');
        output = output.Trim();

        output = InsertBreakPoints(output);
        
        return string.IsNullOrWhiteSpace(output) ? original : output;
    }

    [GeneratedRegex(@"\s[a-zA-z]-?[0-9]{1,2}")]
    private static partial Regex SeatRegex();
    
    private static string InsertBreakPoints(string input)
    {
        var result = SplitBetweenCasing().Replace(input, "\u200B"); // NamePart -> Name​Part
        result = SplitBetweenTextAndNumber().Replace(result, "\u200B"); // Namepart1234 -> Namepart​1234
        result = SplitBetweenNumberAndText().Replace(result, "\u200B"); // 1234Namepart -> 1234​Namepart

        return result;
    }

    [GeneratedRegex("(?<=[a-z])(?=[A-Z])")]
    private static partial Regex SplitBetweenCasing();
    
    [GeneratedRegex("(?<=[a-zA-Z])(?=[0-9])")]
    private static partial Regex SplitBetweenTextAndNumber();
    
    [GeneratedRegex("(?<=[0-9])(?=[a-zA-Z])")]
    private static partial Regex SplitBetweenNumberAndText();
}