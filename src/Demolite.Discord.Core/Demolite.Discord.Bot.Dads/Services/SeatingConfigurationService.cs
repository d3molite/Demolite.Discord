using Demolite.Discord.Bot.Dads.Services;
using LiteDB;

public sealed class SeatingConfigurationService(LiteDatabase db, LanMemberHandler handler)
{
    private readonly ILiteCollection<SeatingRow> _rows = db.GetCollection<SeatingRow>("rows");

    public List<SeatingRow> Rows { get; private set; } = [];

    public event EventHandler? SeatingDataUpdated;

    public void LoadRows()
    {
        Rows = _rows.FindAll().OrderBy(r => r.Letter).ToList();
        NotifyUpdated();
    }

    public void AddRow(string letter, int startingSeat)
    {
        var row = new SeatingRow { Letter = letter, StartingSeat = startingSeat };
        _rows.Insert(row);
        Rows.Add(row);
        NotifyUpdated();
    }

    public void UpdateRow(SeatingRow row)
    {
        _rows.Update(row);
        UnassignInvalidMemberSeats();
        NotifyUpdated();
    }

    public void RemoveRow(SeatingRow row)
    {
        _rows.Delete(row.Id);
        Rows.Remove(row);
        UnassignInvalidMemberSeats();
        NotifyUpdated();
    }
    
    private void UnassignInvalidMemberSeats()
    {
        var validSeats = Rows.SelectMany(r => r.GetSeats()).Select(s => s.Identifier).ToHashSet();
        var affected = handler.LanMembers
            .Where(m => !string.IsNullOrEmpty(m.Seat) && !validSeats.Contains(m.Seat))
            .ToList();

        if (affected.Count == 0) 
            return;

        foreach (var member in affected)
            member.Seat = string.Empty;

        handler.SaveLanMembers();
    }

    public void NotifyUpdated() => 
        SeatingDataUpdated?.Invoke(this, EventArgs.Empty);
}