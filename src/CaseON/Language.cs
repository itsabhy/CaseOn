using System.Collections.Generic;

namespace CaseON;

/// <summary>Writing systems that <see cref="Language.Script(string)"/> can identify.</summary>
public enum ScriptKind
{
    /// <summary>No dominant script detected.</summary>
    Unknown = 0,
    /// <summary>Latin script.</summary>
    Latin,
    /// <summary>Cyrillic script.</summary>
    Cyrillic,
    /// <summary>Greek script.</summary>
    Greek,
    /// <summary>Arabic script.</summary>
    Arabic,
    /// <summary>Hebrew script.</summary>
    Hebrew,
    /// <summary>Han (Chinese) ideographs.</summary>
    Han,
    /// <summary>Japanese kana (Hiragana/Katakana).</summary>
    Kana,
    /// <summary>Korean Hangul.</summary>
    Hangul,
    /// <summary>Devanagari script.</summary>
    Devanagari,
    /// <summary>Thai script.</summary>
    Thai,
}

/// <summary>A best-effort language identification result.</summary>
/// <param name="Name">The English language name (e.g. <c>English</c>).</param>
/// <param name="IsoCode">The ISO 639-1 code (e.g. <c>en</c>).</param>
/// <param name="Confidence">A heuristic confidence in the range <c>[0, 1]</c>.</param>
public sealed record LanguageGuess(string Name, string IsoCode, double Confidence);

/// <summary>
/// Heuristic language and script detection. Call as <c>CaseON.Language.Detect(text)</c> or, after
/// <c>using CaseON;</c>, simply <c>Language.Detect(text)</c>.
/// </summary>
/// <remarks>
/// Detection combines Unicode script analysis with stop-word frequencies for several common
/// Latin-script languages (English, Spanish, French, German, Italian, Portuguese, Dutch). It is a
/// lightweight heuristic, not a statistical model, and is best suited to coarse classification.
/// </remarks>
public static class Language
{
    private static readonly (string Name, string Iso, HashSet<string> Words)[] LatinStopWords =
    [
        ("English", "en", Set("the", "and", "is", "in", "to", "of", "a", "that", "it", "for", "you", "with", "on", "this", "are")),
        ("Spanish", "es", Set("el", "la", "de", "que", "y", "en", "un", "los", "se", "no", "una", "por", "con", "para", "es")),
        ("French", "fr", Set("le", "la", "les", "de", "et", "un", "une", "des", "que", "qui", "dans", "pour", "pas", "vous", "est")),
        ("German", "de", Set("der", "die", "das", "und", "ist", "ein", "eine", "zu", "den", "mit", "nicht", "von", "auf", "für", "im")),
        ("Italian", "it", Set("il", "la", "che", "di", "e", "un", "una", "per", "non", "con", "sono", "del", "le", "gli", "questo")),
        ("Portuguese", "pt", Set("o", "a", "de", "que", "e", "do", "da", "em", "um", "para", "com", "nao", "uma", "os", "como")),
        ("Dutch", "nl", Set("de", "het", "een", "en", "van", "ik", "te", "dat", "die", "in", "niet", "met", "is", "op", "voor")),
    ];

    /// <summary>Determines the dominant writing system used in <paramref name="text"/>.</summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>The most common <see cref="ScriptKind"/>, or <see cref="ScriptKind.Unknown"/> for empty or non-letter text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static ScriptKind Script(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Span<int> counts = stackalloc int[Enum.GetValues<ScriptKind>().Length];
        foreach (var rune in text.EnumerateRunes())
        {
            var kind = Classify(rune.Value);
            if (kind != ScriptKind.Unknown)
            {
                counts[(int)kind]++;
            }
        }

        var best = ScriptKind.Unknown;
        var bestCount = 0;
        for (var i = 1; i < counts.Length; i++)
        {
            if (counts[i] > bestCount)
            {
                bestCount = counts[i];
                best = (ScriptKind)i;
            }
        }

        return best;
    }

    /// <summary>Identifies the most likely language of <paramref name="text"/>.</summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A <see cref="LanguageGuess"/>; the confidence is <c>0</c> when no signal is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static LanguageGuess Detect(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var script = Script(text);
        switch (script)
        {
            case ScriptKind.Cyrillic:
                return new LanguageGuess("Russian", "ru", 0.7);
            case ScriptKind.Greek:
                return new LanguageGuess("Greek", "el", 0.9);
            case ScriptKind.Arabic:
                return new LanguageGuess("Arabic", "ar", 0.8);
            case ScriptKind.Hebrew:
                return new LanguageGuess("Hebrew", "he", 0.9);
            case ScriptKind.Han:
                return new LanguageGuess("Chinese", "zh", 0.7);
            case ScriptKind.Kana:
                return new LanguageGuess("Japanese", "ja", 0.9);
            case ScriptKind.Hangul:
                return new LanguageGuess("Korean", "ko", 0.95);
            case ScriptKind.Devanagari:
                return new LanguageGuess("Hindi", "hi", 0.7);
            case ScriptKind.Thai:
                return new LanguageGuess("Thai", "th", 0.95);
            case ScriptKind.Unknown:
                return new LanguageGuess("Unknown", "und", 0);
            default:
                return DetectLatin(text);
        }
    }

    private static LanguageGuess DetectLatin(string text)
    {
        var words = Tokenize.Words(text);
        if (words.Length == 0)
        {
            return new LanguageGuess("Unknown", "und", 0);
        }

        Span<int> hits = stackalloc int[LatinStopWords.Length];
        foreach (var word in words)
        {
            var lower = word.ToLowerInvariant();
            for (var i = 0; i < LatinStopWords.Length; i++)
            {
                if (LatinStopWords[i].Words.Contains(lower))
                {
                    hits[i]++;
                }
            }
        }

        var bestIndex = -1;
        var bestHits = 0;
        for (var i = 0; i < hits.Length; i++)
        {
            if (hits[i] > bestHits)
            {
                bestHits = hits[i];
                bestIndex = i;
            }
        }

        if (bestIndex < 0)
        {
            // No stop-word evidence; assume English with low confidence.
            return new LanguageGuess("English", "en", 0.1);
        }

        var (name, iso, _) = LatinStopWords[bestIndex];
        var confidence = Math.Min(1.0, (double)bestHits / words.Length * 3);
        return new LanguageGuess(name, iso, confidence);
    }

    private static ScriptKind Classify(int codePoint) => codePoint switch
    {
        >= 0x0041 and <= 0x024F => ScriptKind.Latin,
        >= 0x0370 and <= 0x03FF => ScriptKind.Greek,
        >= 0x0400 and <= 0x04FF => ScriptKind.Cyrillic,
        >= 0x0590 and <= 0x05FF => ScriptKind.Hebrew,
        >= 0x0600 and <= 0x06FF => ScriptKind.Arabic,
        >= 0x0900 and <= 0x097F => ScriptKind.Devanagari,
        >= 0x0E00 and <= 0x0E7F => ScriptKind.Thai,
        >= 0x3040 and <= 0x30FF => ScriptKind.Kana,
        >= 0x4E00 and <= 0x9FFF => ScriptKind.Han,
        >= 0xAC00 and <= 0xD7A3 => ScriptKind.Hangul,
        _ => ScriptKind.Unknown,
    };

    private static HashSet<string> Set(params string[] words) => new(words, StringComparer.Ordinal);
}
