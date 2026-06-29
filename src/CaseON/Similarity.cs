using System.Buffers;
using System.Collections.Generic;

namespace CaseON;

/// <summary>
/// String-similarity and edit-distance algorithms. Call as <c>CaseON.Similarity.Levenshtein("a", "b")</c>
/// or, after <c>using CaseON;</c>, simply <c>Similarity.Levenshtein("a", "b")</c>.
/// </summary>
/// <remarks>
/// <para>
/// Normalized similarity methods return a score in the inclusive range <c>[0, 1]</c>, where <c>1</c>
/// means the inputs are identical and <c>0</c> means maximally different. Comparisons are ordinal
/// (case-sensitive) unless an overload states otherwise. All methods throw
/// <see cref="ArgumentNullException"/> when either argument is <see langword="null"/>.
/// </para>
/// <para>
/// The dynamic-programming algorithms use rolling rows of working memory (O(min(n, m)) space), backed
/// by the stack for small inputs and by <see cref="ArrayPool{T}"/> for larger ones.
/// </para>
/// </remarks>
public static class Similarity
{
    private const int StackallocThreshold = 256;

    /// <summary>Computes the Levenshtein edit distance between two strings.</summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>The minimum number of single-character insertions, deletions or substitutions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int LevenshteinDistance(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        return LevenshteinCore(source, target);
    }

    /// <summary>Computes the Levenshtein edit distance between two character spans without allocating intermediate strings.</summary>
    /// <param name="source">The source characters.</param>
    /// <param name="target">The target characters.</param>
    /// <returns>The minimum number of single-character insertions, deletions or substitutions.</returns>
    public static int LevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target) =>
        LevenshteinCore(source, target);

    /// <summary>Computes Levenshtein similarity, normalized to <c>[0, 1]</c>.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score where <c>1</c> means identical and <c>0</c> means maximally different.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Levenshtein(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var maxLength = Math.Max(source.Length, target.Length);
        return maxLength == 0 ? 1.0 : 1.0 - ((double)LevenshteinCore(source, target) / maxLength);
    }

    /// <summary>Computes Levenshtein similarity between two character spans, normalized to <c>[0, 1]</c>.</summary>
    /// <param name="source">The source characters.</param>
    /// <param name="target">The target characters.</param>
    /// <returns>A similarity score where <c>1</c> means identical and <c>0</c> means maximally different.</returns>
    public static double Levenshtein(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        var maxLength = Math.Max(source.Length, target.Length);
        return maxLength == 0 ? 1.0 : 1.0 - ((double)LevenshteinCore(source, target) / maxLength);
    }

    /// <summary>Computes the Damerau-Levenshtein (optimal string alignment) distance, which also counts adjacent transpositions.</summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>The edit distance including transpositions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int DamerauLevenshteinDistance(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Length == 0)
        {
            return target.Length;
        }

        if (target.Length == 0)
        {
            return source.Length;
        }

        var n = source.Length;
        var m = target.Length;
        var width = m + 1;

        int[]? r0 = null;
        int[]? r1 = null;
        int[]? r2 = null;
        Span<int> prev2 = width <= StackallocThreshold ? stackalloc int[width] : (r0 = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);
        Span<int> prev = width <= StackallocThreshold ? stackalloc int[width] : (r1 = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);
        Span<int> current = width <= StackallocThreshold ? stackalloc int[width] : (r2 = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);

        try
        {
            for (var j = 0; j < width; j++)
            {
                prev[j] = j;
            }

            for (var i = 1; i <= n; i++)
            {
                current[0] = i;
                var sourceChar = source[i - 1];

                for (var j = 1; j <= m; j++)
                {
                    var cost = sourceChar == target[j - 1] ? 0 : 1;
                    var value = Math.Min(Math.Min(prev[j] + 1, current[j - 1] + 1), prev[j - 1] + cost);

                    if (i > 1 && j > 1 && sourceChar == target[j - 2] && source[i - 2] == target[j - 1])
                    {
                        value = Math.Min(value, prev2[j - 2] + 1);
                    }

                    current[j] = value;
                }

                var tmp = prev2;
                prev2 = prev;
                prev = current;
                current = tmp;
            }

            return prev[m];
        }
        finally
        {
            if (r0 is not null)
            {
                ArrayPool<int>.Shared.Return(r0);
            }

            if (r1 is not null)
            {
                ArrayPool<int>.Shared.Return(r1);
            }

            if (r2 is not null)
            {
                ArrayPool<int>.Shared.Return(r2);
            }
        }
    }

    /// <summary>Computes Damerau-Levenshtein similarity, normalized to <c>[0, 1]</c>.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Damerau(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var maxLength = Math.Max(source.Length, target.Length);
        return maxLength == 0 ? 1.0 : 1.0 - ((double)DamerauLevenshteinDistance(source, target) / maxLength);
    }

    /// <summary>Computes the Hamming distance: the number of positions at which the strings differ.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>The number of differing positions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The strings have different lengths.</exception>
    public static int HammingDistance(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Length != target.Length)
        {
            throw new ArgumentException("Hamming distance requires equal-length strings.", nameof(target));
        }

        var distance = 0;
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] != target[i])
            {
                distance++;
            }
        }

        return distance;
    }

    /// <summary>Computes Hamming similarity, normalized to <c>[0, 1]</c>.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The strings have different lengths.</exception>
    public static double Hamming(string source, string target)
    {
        var distance = HammingDistance(source, target);
        return source.Length == 0 ? 1.0 : 1.0 - ((double)distance / source.Length);
    }

    /// <summary>Computes the Jaro similarity between two strings.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Jaro(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (string.Equals(source, target, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (source.Length == 0 || target.Length == 0)
        {
            return 0.0;
        }

        return JaroSimilarity(source, target);
    }

    /// <summary>Computes the Jaro-Winkler similarity, which boosts the score for strings sharing a common prefix.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double JaroWinkler(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (string.Equals(source, target, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (source.Length == 0 || target.Length == 0)
        {
            return 0.0;
        }

        var jaro = JaroSimilarity(source, target);
        var maxPrefix = Math.Min(4, Math.Min(source.Length, target.Length));
        var prefixLength = 0;
        for (var i = 0; i < maxPrefix && source[i] == target[i]; i++)
        {
            prefixLength++;
        }

        const double scalingFactor = 0.1;
        return jaro + (prefixLength * scalingFactor * (1 - jaro));
    }

    /// <summary>Computes the length of the longest common subsequence shared by two strings.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>The length of the longest common subsequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int LongestCommonSubsequenceLength(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Length == 0 || target.Length == 0)
        {
            return 0;
        }

        if (target.Length > source.Length)
        {
            (source, target) = (target, source);
        }

        var width = target.Length + 1;
        int[]? rentedPrevious = null;
        int[]? rentedCurrent = null;

        Span<int> previous = width <= StackallocThreshold
            ? stackalloc int[width]
            : (rentedPrevious = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);
        Span<int> current = width <= StackallocThreshold
            ? stackalloc int[width]
            : (rentedCurrent = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);

        try
        {
            previous.Clear();

            for (var i = 1; i <= source.Length; i++)
            {
                current[0] = 0;
                var sourceChar = source[i - 1];

                for (var j = 1; j < width; j++)
                {
                    current[j] = sourceChar == target[j - 1]
                        ? previous[j - 1] + 1
                        : Math.Max(previous[j], current[j - 1]);
                }

                var swap = previous;
                previous = current;
                current = swap;
            }

            return previous[target.Length];
        }
        finally
        {
            if (rentedPrevious is not null)
            {
                ArrayPool<int>.Shared.Return(rentedPrevious);
            }

            if (rentedCurrent is not null)
            {
                ArrayPool<int>.Shared.Return(rentedCurrent);
            }
        }
    }

    /// <summary>Computes longest-common-subsequence similarity, normalized to <c>[0, 1]</c>.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Lcs(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var maxLength = Math.Max(source.Length, target.Length);
        return maxLength == 0 ? 1.0 : (double)LongestCommonSubsequenceLength(source, target) / maxLength;
    }

    /// <summary>Computes the Sørensen-Dice coefficient over character bigrams.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double SorensenDice(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (string.Equals(source, target, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (source.Length < 2 || target.Length < 2)
        {
            return 0.0;
        }

        var first = BigramCounts(source);
        var second = BigramCounts(target);

        var intersection = 0;
        foreach (var pair in first)
        {
            if (second.TryGetValue(pair.Key, out var count))
            {
                intersection += Math.Min(pair.Value, count);
            }
        }

        return 2.0 * intersection / ((source.Length - 1) + (target.Length - 1));
    }

    /// <summary>Computes the Jaccard index over whitespace-delimited tokens.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Jaccard(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var a = new HashSet<string>(source.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
        var b = new HashSet<string>(target.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);

        if (a.Count == 0 && b.Count == 0)
        {
            return 1.0;
        }

        var intersection = a.Count(b.Contains);
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    /// <summary>Computes cosine similarity over whitespace-delimited token frequencies.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Cosine(string source, string target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var a = TokenCounts(source);
        var b = TokenCounts(target);
        if (a.Count == 0 || b.Count == 0)
        {
            return a.Count == 0 && b.Count == 0 ? 1.0 : 0.0;
        }

        double dot = 0;
        foreach (var pair in a)
        {
            if (b.TryGetValue(pair.Key, out var other))
            {
                dot += pair.Value * (double)other;
            }
        }

        double magA = 0;
        foreach (var v in a.Values)
        {
            magA += (double)v * v;
        }

        double magB = 0;
        foreach (var v in b.Values)
        {
            magB += (double)v * v;
        }

        var denom = Math.Sqrt(magA) * Math.Sqrt(magB);
        return denom == 0 ? 0.0 : dot / denom;
    }

    /// <summary>Computes a normalized similarity score using the specified <paramref name="algorithm"/>.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <param name="algorithm">The algorithm to use.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Compute(string source, string target, SimilarityAlgorithm algorithm = SimilarityAlgorithm.Levenshtein) => algorithm switch
    {
        SimilarityAlgorithm.Levenshtein => Levenshtein(source, target),
        SimilarityAlgorithm.DamerauLevenshtein => Damerau(source, target),
        SimilarityAlgorithm.Jaro => Jaro(source, target),
        SimilarityAlgorithm.JaroWinkler => JaroWinkler(source, target),
        SimilarityAlgorithm.Hamming => Hamming(source, target),
        SimilarityAlgorithm.Lcs => Lcs(source, target),
        SimilarityAlgorithm.SorensenDice => SorensenDice(source, target),
        SimilarityAlgorithm.Jaccard => Jaccard(source, target),
        SimilarityAlgorithm.Cosine => Cosine(source, target),
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Unknown similarity algorithm."),
    };

    /// <summary>Computes a normalized similarity score using the specified <paramref name="options"/>.</summary>
    /// <param name="source">The first string.</param>
    /// <param name="target">The second string.</param>
    /// <param name="options">The comparison options.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
    public static double Compute(string source, string target, MatchOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (options.IgnoreCase)
        {
            source = source.ToUpperInvariant();
            target = target.ToUpperInvariant();
        }

        return Compute(source, target, options.Algorithm);
    }

    private static int LevenshteinCore(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        if (source.Length == 0)
        {
            return target.Length;
        }

        if (target.Length == 0)
        {
            return source.Length;
        }

        if (target.Length > source.Length)
        {
            var swapSpan = source;
            source = target;
            target = swapSpan;
        }

        var width = target.Length + 1;
        int[]? rentedPrevious = null;
        int[]? rentedCurrent = null;

        Span<int> previous = width <= StackallocThreshold
            ? stackalloc int[width]
            : (rentedPrevious = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);
        Span<int> current = width <= StackallocThreshold
            ? stackalloc int[width]
            : (rentedCurrent = ArrayPool<int>.Shared.Rent(width)).AsSpan(0, width);

        try
        {
            for (var j = 0; j < width; j++)
            {
                previous[j] = j;
            }

            for (var i = 1; i <= source.Length; i++)
            {
                current[0] = i;
                var sourceChar = source[i - 1];

                for (var j = 1; j < width; j++)
                {
                    var cost = sourceChar == target[j - 1] ? 0 : 1;
                    current[j] = Math.Min(
                        Math.Min(previous[j] + 1, current[j - 1] + 1),
                        previous[j - 1] + cost);
                }

                var swap = previous;
                previous = current;
                current = swap;
            }

            return previous[target.Length];
        }
        finally
        {
            if (rentedPrevious is not null)
            {
                ArrayPool<int>.Shared.Return(rentedPrevious);
            }

            if (rentedCurrent is not null)
            {
                ArrayPool<int>.Shared.Return(rentedCurrent);
            }
        }
    }

    private static Dictionary<string, int> BigramCounts(string value)
    {
        var counts = new Dictionary<string, int>(value.Length, StringComparer.Ordinal);
        for (var i = 0; i < value.Length - 1; i++)
        {
            var bigram = value.Substring(i, 2);
            counts[bigram] = counts.TryGetValue(bigram, out var c) ? c + 1 : 1;
        }

        return counts;
    }

    private static Dictionary<string, int> TokenCounts(string value)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            counts[token] = counts.TryGetValue(token, out var c) ? c + 1 : 1;
        }

        return counts;
    }

    private static double JaroSimilarity(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var matchDistance = Math.Max(0, (Math.Max(len1, len2) / 2) - 1);

        var s1Matches = new bool[len1];
        var s2Matches = new bool[len2];
        var matches = 0;

        for (var i = 0; i < len1; i++)
        {
            var start = Math.Max(0, i - matchDistance);
            var end = Math.Min(len2, i + matchDistance + 1);
            for (var j = start; j < end; j++)
            {
                if (s2Matches[j] || s1[i] != s2[j])
                {
                    continue;
                }

                s1Matches[i] = true;
                s2Matches[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0)
        {
            return 0.0;
        }

        double transpositions = 0;
        var k = 0;
        for (var i = 0; i < len1; i++)
        {
            if (!s1Matches[i])
            {
                continue;
            }

            while (!s2Matches[k])
            {
                k++;
            }

            if (s1[i] != s2[k])
            {
                transpositions++;
            }

            k++;
        }

        transpositions /= 2;
        return ((matches / (double)len1) + (matches / (double)len2) + ((matches - transpositions) / matches)) / 3.0;
    }
}
