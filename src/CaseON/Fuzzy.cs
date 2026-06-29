using System.Collections.Generic;
using System.Linq;

namespace CaseON;

/// <summary>The result of a fuzzy match against a candidate value.</summary>
/// <param name="Value">The candidate string.</param>
/// <param name="Score">The similarity score in the range <c>[0, 1]</c>.</param>
/// <param name="Index">The zero-based position of the candidate in the source sequence.</param>
public readonly record struct MatchResult(string Value, double Score, int Index);

/// <summary>
/// Fuzzy string matching over candidate collections. Call as
/// <c>CaseON.Fuzzy.BestMatch("aple", new[] { "apple", "grape" })</c> or, after <c>using CaseON;</c>,
/// simply <c>Fuzzy.BestMatch(...)</c>.
/// </summary>
/// <remarks>Scoring defaults to <see cref="SimilarityAlgorithm.JaroWinkler"/>, which works well for short strings and typos.</remarks>
public static class Fuzzy
{
    private const SimilarityAlgorithm DefaultAlgorithm = SimilarityAlgorithm.JaroWinkler;

    /// <summary>Scores how closely <paramref name="candidate"/> matches <paramref name="query"/>.</summary>
    /// <param name="query">The search query.</param>
    /// <param name="candidate">The candidate to score.</param>
    /// <param name="algorithm">The similarity algorithm to use. Defaults to Jaro-Winkler.</param>
    /// <returns>A similarity score in the range <c>[0, 1]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> or <paramref name="candidate"/> is <see langword="null"/>.</exception>
    public static double Score(string query, string candidate, SimilarityAlgorithm algorithm = DefaultAlgorithm) =>
        Similarity.Compute(query, candidate, new MatchOptions(algorithm, IgnoreCase: true));

    /// <summary>Determines whether <paramref name="candidate"/> matches <paramref name="query"/> at or above a threshold.</summary>
    /// <param name="query">The search query.</param>
    /// <param name="candidate">The candidate to test.</param>
    /// <param name="threshold">The minimum score (inclusive) required, in the range <c>[0, 1]</c>. Defaults to 0.7.</param>
    /// <param name="algorithm">The similarity algorithm to use. Defaults to Jaro-Winkler.</param>
    /// <returns><see langword="true"/> if the candidate scores at or above <paramref name="threshold"/>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> or <paramref name="candidate"/> is <see langword="null"/>.</exception>
    public static bool IsMatch(string query, string candidate, double threshold = 0.7, SimilarityAlgorithm algorithm = DefaultAlgorithm) =>
        Score(query, candidate, algorithm) >= threshold;

    /// <summary>Finds the best-scoring candidate for <paramref name="query"/>.</summary>
    /// <param name="query">The search query.</param>
    /// <param name="candidates">The candidates to score.</param>
    /// <param name="threshold">The minimum score (inclusive) a candidate must reach to be returned. Defaults to 0.</param>
    /// <param name="algorithm">The similarity algorithm to use. Defaults to Jaro-Winkler.</param>
    /// <returns>The highest-scoring <see cref="MatchResult"/>, or <see langword="null"/> if none reach the threshold.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> or <paramref name="candidates"/> is <see langword="null"/>.</exception>
    public static MatchResult? BestMatch(string query, IEnumerable<string> candidates, double threshold = 0, SimilarityAlgorithm algorithm = DefaultAlgorithm)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(candidates);

        MatchResult? best = null;
        var index = 0;
        foreach (var candidate in candidates)
        {
            if (candidate is not null)
            {
                var score = Score(query, candidate, algorithm);
                if (score >= threshold && (best is null || score > best.Value.Score))
                {
                    best = new MatchResult(candidate, score, index);
                }
            }

            index++;
        }

        return best;
    }

    /// <summary>Scores and ranks all candidates against <paramref name="query"/>, best first.</summary>
    /// <param name="query">The search query.</param>
    /// <param name="candidates">The candidates to rank.</param>
    /// <param name="algorithm">The similarity algorithm to use. Defaults to Jaro-Winkler.</param>
    /// <returns>The candidates as <see cref="MatchResult"/> values ordered by descending score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> or <paramref name="candidates"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<MatchResult> RankMatches(string query, IEnumerable<string> candidates, SimilarityAlgorithm algorithm = DefaultAlgorithm) =>
        Search(query, candidates, threshold: 0, algorithm);

    /// <summary>Returns candidates that match <paramref name="query"/> at or above a threshold, best first.</summary>
    /// <param name="query">The search query.</param>
    /// <param name="candidates">The candidates to search.</param>
    /// <param name="threshold">The minimum score (inclusive) required, in the range <c>[0, 1]</c>. Defaults to 0.5.</param>
    /// <param name="algorithm">The similarity algorithm to use. Defaults to Jaro-Winkler.</param>
    /// <returns>The matching candidates as <see cref="MatchResult"/> values ordered by descending score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> or <paramref name="candidates"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<MatchResult> Search(string query, IEnumerable<string> candidates, double threshold = 0.5, SimilarityAlgorithm algorithm = DefaultAlgorithm)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(candidates);

        var results = new List<MatchResult>();
        var index = 0;
        foreach (var candidate in candidates)
        {
            if (candidate is not null)
            {
                var score = Score(query, candidate, algorithm);
                if (score >= threshold)
                {
                    results.Add(new MatchResult(candidate, score, index));
                }
            }

            index++;
        }

        results.Sort(static (a, b) => b.Score.CompareTo(a.Score));
        return results;
    }
}
