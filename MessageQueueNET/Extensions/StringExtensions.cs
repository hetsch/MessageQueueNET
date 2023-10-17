namespace MessageQueueNET.Extensions;

public static class StringExtensions
{
    public static bool IsPattern(this string pattern)
        => string.IsNullOrEmpty(pattern) ? false : pattern.Contains("*");

    public static string ToRegexPattern(this string pattern)
        // Replace '*' with '.*' to convert it into a Regex pattern
        => $"^{pattern.Replace(" *", ".*")}$"; 

    public static bool FitsPattern(this string aString, string pattern)
        => aString.FitsRegexPattern(pattern.ToRegexPattern());

    public static bool FitsRegexPattern(this string aString, string regexPattern)
        // Use Regex.IsMatch to check if the string matches the pattern
        => System.Text.RegularExpressions.Regex.IsMatch(aString, regexPattern);
}
