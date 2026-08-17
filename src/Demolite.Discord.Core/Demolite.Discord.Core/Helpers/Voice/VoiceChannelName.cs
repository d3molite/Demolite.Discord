using System.Text.RegularExpressions;

namespace Demolite.Discord.Core.Helpers.Voice;

public static partial class VoiceChannelName
{
    [GeneratedRegex(@"^(\d+) \| ")]
    private static partial Regex NumberPrefix();

    public static string Format(int number, string name) => $"{number} | {name}";

    public static string StripNumber(string channelName) => NumberPrefix().Replace(channelName, "");

    public static bool TryParseNumber(string channelName, out int number)
    {
        var match = NumberPrefix().Match(channelName);
        number = match.Success ? int.Parse(match.Groups[1].Value) : 0;
        return match.Success;
    }
}