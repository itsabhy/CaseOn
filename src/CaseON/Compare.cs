using System.Collections.Generic;

namespace CaseON;

/// <summary>
/// String comparison helpers covering ordinal/invariant equality, prefix/suffix/substring checks and
/// natural (human) ordering. Call as <c>CaseON.Compare.Equals("a", "A", ignoreCase: true)</c> or, after
/// <c>using CaseON;</c>, simply <c>Compare.Equals(...)</c>.
/// </summary>
public static class Compare
{
    /// <summary>Compares two strings for ordinal equality.</summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, comparison ignores case.</param>
    /// <returns><see langword="true"/> if the strings are equal; otherwise <see langword="false"/>.</returns>
    public static bool Equals(string? first, string? second, bool ignoreCase = false) =>
        string.Equals(first, second, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    /// <summary>Compares two strings for culture-invariant equality.</summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, comparison ignores case.</param>
    /// <returns><see langword="true"/> if the strings are equal; otherwise <see langword="false"/>.</returns>
    public static bool EqualsInvariant(string? first, string? second, bool ignoreCase = false) =>
        string.Equals(first, second, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);

    /// <summary>Determines whether <paramref name="value"/> starts with <paramref name="prefix"/> (ordinal).</summary>
    /// <param name="value">The string to test.</param>
    /// <param name="prefix">The prefix to look for.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, comparison ignores case.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> starts with <paramref name="prefix"/>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="prefix"/> is <see langword="null"/>.</exception>
    public static bool StartsWith(string value, string prefix, bool ignoreCase = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(prefix);
        return value.StartsWith(prefix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>Determines whether <paramref name="value"/> ends with <paramref name="suffix"/> (ordinal).</summary>
    /// <param name="value">The string to test.</param>
    /// <param name="suffix">The suffix to look for.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, comparison ignores case.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> ends with <paramref name="suffix"/>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="suffix"/> is <see langword="null"/>.</exception>
    public static bool EndsWith(string value, string suffix, bool ignoreCase = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(suffix);
        return value.EndsWith(suffix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>Determines whether <paramref name="value"/> contains <paramref name="substring"/> (ordinal).</summary>
    /// <param name="value">The string to test.</param>
    /// <param name="substring">The substring to look for.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, comparison ignores case.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> contains <paramref name="substring"/>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="substring"/> is <see langword="null"/>.</exception>
    public static bool Contains(string value, string substring, bool ignoreCase = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(substring);
        return value.Contains(substring, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>An <see cref="IComparer{T}"/> that orders strings using natural (human) ordering.</summary>
    /// <remarks>For example, <c>file2</c> sorts before <c>file10</c> because embedded numbers are compared numerically.</remarks>
    public static IComparer<string> NaturalComparer { get; } = new NaturalStringComparer(ignoreCase: false);

    /// <summary>An <see cref="IComparer{T}"/> that orders strings using case-insensitive natural ordering.</summary>
    public static IComparer<string> NaturalComparerIgnoreCase { get; } = new NaturalStringComparer(ignoreCase: true);

    /// <summary>Compares two strings using natural (human) ordering, where embedded numbers are compared numerically.</summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, comparison ignores case.</param>
    /// <returns>A negative value if <paramref name="first"/> sorts first, zero if equal, otherwise a positive value.</returns>
    /// <example><code>Compare.NaturalCompare("file2", "file10"); // negative (file2 before file10)</code></example>
    public static int NaturalCompare(string? first, string? second, bool ignoreCase = false)
    {
        if (ReferenceEquals(first, second))
        {
            return 0;
        }

        if (first is null)
        {
            return -1;
        }

        if (second is null)
        {
            return 1;
        }

        var i = 0;
        var j = 0;
        while (i < first.Length && j < second.Length)
        {
            var a = first[i];
            var b = second[j];

            if (char.IsDigit(a) && char.IsDigit(b))
            {
                var startA = i;
                var startB = j;
                while (i < first.Length && char.IsDigit(first[i]))
                {
                    i++;
                }

                while (j < second.Length && char.IsDigit(second[j]))
                {
                    j++;
                }

                var numA = first.AsSpan(startA, i - startA).TrimStart('0');
                var numB = second.AsSpan(startB, j - startB).TrimStart('0');

                if (numA.Length != numB.Length)
                {
                    return numA.Length - numB.Length;
                }

                var cmp = numA.CompareTo(numB, StringComparison.Ordinal);
                if (cmp != 0)
                {
                    return cmp;
                }
            }
            else
            {
                var ca = ignoreCase ? char.ToUpperInvariant(a) : a;
                var cb = ignoreCase ? char.ToUpperInvariant(b) : b;
                if (ca != cb)
                {
                    return ca - cb;
                }

                i++;
                j++;
            }
        }

        return (first.Length - i) - (second.Length - j);
    }

    private sealed class NaturalStringComparer(bool ignoreCase) : IComparer<string>
    {
        public int Compare(string? x, string? y) => NaturalCompare(x, y, ignoreCase);
    }
}
