using System.Collections.Generic;
using System.Text.RegularExpressions;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// Convenience wrappers over <see cref="Regex"/> with safe defaults. Call as
/// <c>CaseON.RegexTools.Matches(text, pattern)</c> or, after <c>using CaseON;</c>, simply
/// <c>RegexTools.Matches(...)</c>.
/// </summary>
/// <remarks>Patterns are compiled once and cached, evaluated with a one-second timeout, and are culture-invariant.</remarks>
public static class RegexTools
{
    /// <summary>Escapes regex metacharacters in <paramref name="input"/>.</summary>
    /// <param name="input">The literal text to escape.</param>
    /// <returns>The escaped text, safe to embed in a pattern.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Escape(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Regex.Escape(input);
    }

    /// <summary>Unescapes a regex-escaped <paramref name="input"/>.</summary>
    /// <param name="input">The escaped text to unescape.</param>
    /// <returns>The unescaped text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Unescape(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Regex.Unescape(input);
    }

    /// <summary>Determines whether <paramref name="input"/> matches <paramref name="pattern"/>.</summary>
    /// <param name="input">The text to test.</param>
    /// <param name="pattern">The regular-expression pattern.</param>
    /// <returns><see langword="true"/> if the pattern matches; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static bool IsMatch(string input, string pattern)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCache.IsMatch(input, pattern);
    }

    /// <summary>Replaces all matches of <paramref name="pattern"/> in <paramref name="input"/> with <paramref name="replacement"/>.</summary>
    /// <param name="input">The text to transform.</param>
    /// <param name="pattern">The regular-expression pattern.</param>
    /// <param name="replacement">The replacement string (supports substitutions such as <c>$1</c>).</param>
    /// <returns>The transformed text, or the original on regex timeout.</returns>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    public static string Replace(string input, string pattern, string replacement)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(replacement);

        try
        {
            return RegexCache.Get(pattern).Replace(input, replacement);
        }
        catch (RegexMatchTimeoutException)
        {
            return input;
        }
    }

    /// <summary>Returns all matches of <paramref name="pattern"/> in <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <param name="pattern">The regular-expression pattern.</param>
    /// <returns>An array of matched substrings (possibly empty).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static string[] Matches(string input, string pattern)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);

        try
        {
            var matches = RegexCache.Get(pattern).Matches(input);
            var result = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Value;
            }

            return result;
        }
        catch (RegexMatchTimeoutException)
        {
            return [];
        }
    }

    /// <summary>Returns the named capture groups of the first match of <paramref name="pattern"/> in <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <param name="pattern">The regular-expression pattern containing named groups.</param>
    /// <returns>A dictionary mapping group names to captured values; empty if there is no match.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static IReadOnlyDictionary<string, string> NamedGroups(string input, string pattern)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            var regex = RegexCache.Get(pattern);
            var match = regex.Match(input);
            if (!match.Success)
            {
                return result;
            }

            foreach (var name in regex.GetGroupNames())
            {
                // Skip the implicit numbered groups.
                if (!int.TryParse(name, out _))
                {
                    var group = match.Groups[name];
                    if (group.Success)
                    {
                        result[name] = group.Value;
                    }
                }
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return result;
        }

        return result;
    }
}
