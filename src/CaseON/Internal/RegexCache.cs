using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CaseON.Internal;

/// <summary>
/// Caches <see cref="Regex"/> instances compiled from runtime-supplied patterns (for example
/// per-country postal-code patterns) so that each distinct pattern is built only once. All cached
/// regexes are culture-invariant and guarded with a one-second match timeout.
/// </summary>
internal static class RegexCache
{
    private static readonly ConcurrentDictionary<string, Regex> Cache = new(StringComparer.Ordinal);

    public static Regex Get(string pattern) => Cache.GetOrAdd(pattern, static p =>
        new Regex(p, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(1000)));

    public static bool IsMatch(string input, string pattern)
    {
        var regex = Get(pattern);

        try
        {
            return regex.IsMatch(input);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
