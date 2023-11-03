using System.Text.RegularExpressions;

namespace MessageQueueNET.Extensions;

public static class StringExtensions
{
    public static bool IsPattern(this string pattern)
        => string.IsNullOrEmpty(pattern) ? false : pattern.Contains("*") || pattern.Contains(',');

    public static string ToRegexPattern(this string pattern)
        // Replace '*' with '.*' to convert it into a Regex pattern
        => $"^{Regex.Escape(pattern).Replace(@"\*", ".*")}$"; // $"^{pattern.Replace("*", ".*")}$"; 

    public static bool FitsPattern(this string aString, string pattern)
        => aString.FitsRegexPattern(pattern.ToRegexPattern());

    public static bool FitsRegexPattern(this string aString, string regexPattern)
        // Use Regex.IsMatch to check if the string matches the pattern
        => Regex.IsMatch(aString, regexPattern);

    private const string QueueNamePattern = "^[a-z0-9-_.]+$";
    public static bool IsValidQueueName(this string queueName) =>
        Regex.IsMatch(queueName ?? string.Empty, QueueNamePattern);
}
