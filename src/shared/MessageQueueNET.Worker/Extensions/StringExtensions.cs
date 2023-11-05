using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MessageQueueNET.Worker.Extensions;

internal static class StringExtensions
{
    public static string ToRegexPattern(this string pattern)
        // Replace '*' with '.*' to convert it into a Regex pattern
        => $"^{Regex.Escape(pattern).Replace(@"\*", ".*")}$"; // $"^{pattern.Replace("*", ".*")}$"; 

    public static bool FitsPattern(this string aString, string pattern)
        => !String.IsNullOrEmpty(pattern) 
            && aString.FitsRegexPattern(pattern.ToRegexPattern());

    public static bool FitsAnyPattern(this string aString, string[] patterns)
        => patterns
            .Select(p => p.Trim())
            .Any(p => aString.FitsPattern(p));

    public static bool FitsRegexPattern(this string aString, string regexPattern)
        // Use Regex.IsMatch to check if the string matches the pattern
        => Regex.IsMatch(aString, regexPattern);
}
