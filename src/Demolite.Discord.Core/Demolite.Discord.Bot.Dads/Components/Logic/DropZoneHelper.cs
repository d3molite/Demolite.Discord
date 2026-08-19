using Demolite.Discord.Bot.Dads.Services;
using MudBlazor;

namespace Demolite.Discord.Bot.Dads.Components.Logic;

public sealed class DropZoneHelper(LanMemberHandler memberHandler, SeatingConfigurationService seatingService)
{
    private List<LanMember> Members => memberHandler.LanMembers;

    public void ItemUpdated(MudItemDropInfo<LanMember> dropInfo)
    {
        var member = dropInfo.Item!;
        var targetSeat = dropInfo.DropzoneIdentifier;

        var occupant = Members.FirstOrDefault(m => (m.Seat ?? "") == targetSeat && m.Id != member.Id);
        if (occupant is not null)
            occupant.Seat = member.Seat;

        member.Seat = targetSeat;
        memberHandler.SaveLanMembers();
    }

    public bool CanDrop(LanMember member, string targetSeat)
    {
        if (targetSeat == "") return true;
        if (IsBlocked(targetSeat)) return false;
        if (!member.IsDoubleSeat) return true;

        var partnerSeat = GetPartnerSeat(targetSeat);
        if (IsBlocked(partnerSeat)) return false;

        var partnerOccupant = Members.FirstOrDefault(m => (m.Seat ?? "") == partnerSeat && m.Id != member.Id);
        return partnerOccupant is null;
    }

    private bool IsBlocked(string seatIdentifier)
    {
        var parts = seatIdentifier.Split('-');
        var letter = parts[0];
        var number = int.Parse(parts[1]);

        var row = seatingService.Rows.FirstOrDefault(r => r.Letter == letter);
        return row is not null && row.BlockedSeatNumbers.Contains(number);
    }

    private static string GetPartnerSeat(string identifier)
    {
        var parts = identifier.Split('-');
        var letter = parts[0];
        var number = int.Parse(parts[1]);

        var partnerNumber = number % 2 != 0 ? number + 1 : number - 1;
        return $"{letter}-{partnerNumber}";
    }
}