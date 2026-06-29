namespace CaseON;

/// <summary>Selects a string-similarity algorithm for <see cref="Similarity.Compute(string, string, SimilarityAlgorithm)"/>.</summary>
public enum SimilarityAlgorithm
{
    /// <summary>Levenshtein edit distance, normalized to <c>[0, 1]</c>.</summary>
    Levenshtein = 0,

    /// <summary>Damerau-Levenshtein (optimal string alignment) distance, normalized to <c>[0, 1]</c>.</summary>
    DamerauLevenshtein,

    /// <summary>Jaro similarity.</summary>
    Jaro,

    /// <summary>Jaro-Winkler similarity.</summary>
    JaroWinkler,

    /// <summary>Hamming similarity (inputs must be the same length).</summary>
    Hamming,

    /// <summary>Longest common subsequence, normalized to <c>[0, 1]</c>.</summary>
    Lcs,

    /// <summary>Sørensen-Dice coefficient over character bigrams.</summary>
    SorensenDice,

    /// <summary>Jaccard index over whitespace-delimited tokens.</summary>
    Jaccard,

    /// <summary>Cosine similarity over whitespace-delimited token frequencies.</summary>
    Cosine,
}

/// <summary>Options controlling a similarity comparison.</summary>
/// <param name="Algorithm">The algorithm to use.</param>
/// <param name="IgnoreCase">When <see langword="true"/>, inputs are compared case-insensitively.</param>
public readonly record struct MatchOptions(
    SimilarityAlgorithm Algorithm = SimilarityAlgorithm.Levenshtein,
    bool IgnoreCase = false);
