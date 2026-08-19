namespace Demolite.Discord.Bot.Dads.Services;

public enum CarpetPosition
{
    None,
    Top,
    Bottom,
    Both
}

public class SeatingRow
{
    public int Id { get; set; }
    
    public int Order { get; set; }

    public string Letter { get; set; } = string.Empty;

    public int StartingSeat { get; set; }

    public CarpetPosition CarpetPosition { get; set; } = CarpetPosition.None;
    
    public HashSet<int> BlockedSeatNumbers { get; set; } = [];
}

public static class SeatingRowExtensions
{
    public static IReadOnlyList<Seat> GetSeats(this SeatingRow row) =>
        Enumerable.Range(row.StartingSeat, 40)
            .Select(n => new Seat(row.Letter, n, row.BlockedSeatNumbers.Contains(n)))
            .ToList();

    public static void ToggleBlocked(this SeatingRow row, int seatNumber)
    {
        if (!row.BlockedSeatNumbers.Remove(seatNumber))
            row.BlockedSeatNumbers.Add(seatNumber);
    }
}

public readonly record struct Seat(string RowLetter, int SeatNumber, bool IsBlocked = false)
{
    public string Identifier => $"{RowLetter}-{SeatNumber}";
    
    public int TableIndex => (SeatNumber - 1) / 2;

    public bool IsLeftSeat => SeatNumber % 2 != 0;
}