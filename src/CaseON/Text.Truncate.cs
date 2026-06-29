namespace CaseON;

public static partial class Text
{
    private const string DefaultEllipsis = "\u2026"; // …

    /// <summary>Truncates <paramref name="input"/> to at most <paramref name="maxLength"/> characters, appending <paramref name="suffix"/>.</summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the result, including the suffix.</param>
    /// <param name="suffix">The suffix appended when truncation occurs. Defaults to an ellipsis.</param>
    /// <returns>The original string if it already fits; otherwise the truncated string with the suffix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    /// <example><code>Text.Truncate("Hello world", 8); // "Hello w…"</code></example>
    public static string Truncate(string input, int maxLength, string suffix = DefaultEllipsis)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        suffix ??= string.Empty;

        if (input.Length <= maxLength)
        {
            return input;
        }

        var keep = Math.Max(0, maxLength - suffix.Length);
        return string.Concat(TakeChars(input, keep), suffix);
    }

    /// <summary>Truncates <paramref name="input"/> to at most <paramref name="maxWords"/> words.</summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxWords">The maximum number of whitespace-delimited words to keep.</param>
    /// <param name="suffix">The suffix appended when truncation occurs. Defaults to an ellipsis.</param>
    /// <returns>The truncated string, or the original if it has no more than <paramref name="maxWords"/> words.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxWords"/> is negative.</exception>
    /// <example><code>Text.TruncateWords("one two three four", 2); // "one two…"</code></example>
    public static string TruncateWords(string input, int maxWords, string suffix = DefaultEllipsis)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(maxWords);
        suffix ??= string.Empty;

        var words = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxWords)
        {
            return input;
        }

        return string.Concat(string.Join(' ', words.Take(maxWords)), suffix);
    }

    /// <summary>Truncates <paramref name="input"/> to at most <paramref name="maxSentences"/> sentences.</summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxSentences">The maximum number of sentences to keep (terminated by <c>.</c>, <c>!</c> or <c>?</c>).</param>
    /// <param name="suffix">The suffix appended when truncation occurs. Defaults to an ellipsis.</param>
    /// <returns>The truncated string, or the original if it has no more than <paramref name="maxSentences"/> sentences.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxSentences"/> is not positive.</exception>
    public static string TruncateSentences(string input, int maxSentences, string suffix = DefaultEllipsis)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSentences);
        suffix ??= string.Empty;

        var count = 0;
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] is '.' or '!' or '?')
            {
                count++;
                if (count == maxSentences)
                {
                    var end = i + 1;
                    return end >= input.Length ? input : string.Concat(input.AsSpan(0, end), suffix);
                }
            }
        }

        return input;
    }

    /// <summary>Truncates the middle of <paramref name="input"/>, keeping the start and end.</summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the result, including the separator.</param>
    /// <param name="separator">The middle separator. Defaults to an ellipsis.</param>
    /// <returns>The original string if it fits; otherwise the start and end joined by the separator.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    /// <example><code>Text.TruncateMiddle("abcdefghij", 7); // "abc…hij"</code></example>
    public static string TruncateMiddle(string input, int maxLength, string separator = DefaultEllipsis)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        separator ??= string.Empty;

        if (input.Length <= maxLength)
        {
            return input;
        }

        var keep = Math.Max(0, maxLength - separator.Length);
        var left = (keep + 1) / 2;
        var right = keep / 2;

        var leftPart = TakeChars(input, left);
        var rightPart = TakeLastChars(input, right);
        return string.Concat(leftPart, separator, rightPart);
    }

    /// <summary>Appends an ellipsis to <paramref name="input"/> if it exceeds <paramref name="maxLength"/>.</summary>
    /// <param name="input">The string to shorten.</param>
    /// <param name="maxLength">The maximum length of the result, including the ellipsis.</param>
    /// <returns>The shortened string with a trailing ellipsis when needed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static string Ellipsis(string input, int maxLength) => Truncate(input, maxLength, DefaultEllipsis);

    private static string TakeChars(string input, int count)
    {
        if (count >= input.Length)
        {
            return input;
        }

        // Avoid splitting a surrogate pair.
        if (count > 0 && char.IsHighSurrogate(input[count - 1]))
        {
            count--;
        }

        return input[..count];
    }

    private static string TakeLastChars(string input, int count)
    {
        if (count >= input.Length)
        {
            return input;
        }

        var start = input.Length - count;
        if (start > 0 && char.IsLowSurrogate(input[start]))
        {
            start++;
        }

        return input[start..];
    }
}
