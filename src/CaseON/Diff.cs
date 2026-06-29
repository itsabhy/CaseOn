using System.Collections.Generic;

namespace CaseON;

/// <summary>The result of comparing two texts: which tokens were added, removed or left unchanged.</summary>
/// <param name="Added">Tokens present in the new text but not the old.</param>
/// <param name="Removed">Tokens present in the old text but not the new.</param>
/// <param name="Unchanged">Tokens common to both texts, in order.</param>
public sealed record DiffResult(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed,
    IReadOnlyList<string> Unchanged);

/// <summary>
/// Computes a simple longest-common-subsequence diff between two texts. Call as
/// <c>CaseON.Diff.Lines(oldText, newText)</c> or, after <c>using CaseON;</c>, simply
/// <c>Diff.Lines(...)</c>.
/// </summary>
public static class Diff
{
    /// <summary>Compares <paramref name="oldText"/> and <paramref name="newText"/> line by line.</summary>
    /// <param name="oldText">The original text.</param>
    /// <param name="newText">The updated text.</param>
    /// <returns>A <see cref="DiffResult"/> describing the line-level changes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="oldText"/> or <paramref name="newText"/> is <see langword="null"/>.</exception>
    public static DiffResult Lines(string oldText, string newText)
    {
        ArgumentNullException.ThrowIfNull(oldText);
        ArgumentNullException.ThrowIfNull(newText);
        return Compare(Tokenize.Lines(oldText), Tokenize.Lines(newText));
    }

    /// <summary>Compares <paramref name="oldText"/> and <paramref name="newText"/> word by word.</summary>
    /// <param name="oldText">The original text.</param>
    /// <param name="newText">The updated text.</param>
    /// <returns>A <see cref="DiffResult"/> describing the word-level changes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="oldText"/> or <paramref name="newText"/> is <see langword="null"/>.</exception>
    public static DiffResult Words(string oldText, string newText)
    {
        ArgumentNullException.ThrowIfNull(oldText);
        ArgumentNullException.ThrowIfNull(newText);
        return Compare(Tokenize.Words(oldText), Tokenize.Words(newText));
    }

    /// <summary>Compares two token sequences using a longest-common-subsequence diff.</summary>
    /// <param name="oldTokens">The original tokens.</param>
    /// <param name="newTokens">The updated tokens.</param>
    /// <returns>A <see cref="DiffResult"/> describing the changes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="oldTokens"/> or <paramref name="newTokens"/> is <see langword="null"/>.</exception>
    public static DiffResult Compare(IReadOnlyList<string> oldTokens, IReadOnlyList<string> newTokens)
    {
        ArgumentNullException.ThrowIfNull(oldTokens);
        ArgumentNullException.ThrowIfNull(newTokens);

        var n = oldTokens.Count;
        var m = newTokens.Count;

        // Standard LCS table.
        var lengths = new int[n + 1, m + 1];
        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                lengths[i, j] = string.Equals(oldTokens[i - 1], newTokens[j - 1], StringComparison.Ordinal)
                    ? lengths[i - 1, j - 1] + 1
                    : Math.Max(lengths[i - 1, j], lengths[i, j - 1]);
            }
        }

        var added = new List<string>();
        var removed = new List<string>();
        var unchanged = new List<string>();

        // Walk the table backwards to classify each token.
        var x = n;
        var y = m;
        while (x > 0 && y > 0)
        {
            if (string.Equals(oldTokens[x - 1], newTokens[y - 1], StringComparison.Ordinal))
            {
                unchanged.Add(oldTokens[x - 1]);
                x--;
                y--;
            }
            else if (lengths[x - 1, y] >= lengths[x, y - 1])
            {
                removed.Add(oldTokens[x - 1]);
                x--;
            }
            else
            {
                added.Add(newTokens[y - 1]);
                y--;
            }
        }

        while (x > 0)
        {
            removed.Add(oldTokens[--x]);
        }

        while (y > 0)
        {
            added.Add(newTokens[--y]);
        }

        added.Reverse();
        removed.Reverse();
        unchanged.Reverse();
        return new DiffResult(added, removed, unchanged);
    }
}
