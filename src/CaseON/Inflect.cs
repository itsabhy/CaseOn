using System.Collections.Generic;
using System.Globalization;

namespace CaseON;

/// <summary>
/// English inflection: pluralization, singularization and ordinals. Call as
/// <c>CaseON.Inflect.Pluralize("category")</c> or, after <c>using CaseON;</c>, simply
/// <c>Inflect.Pluralize(...)</c>.
/// </summary>
/// <remarks>Rules target English and cover common irregular and uncountable nouns; they are heuristics, not a dictionary.</remarks>
public static class Inflect
{
    private static readonly HashSet<string> Uncountable = new(StringComparer.OrdinalIgnoreCase)
    {
        "equipment", "information", "rice", "money", "species", "series", "fish", "sheep",
        "deer", "aircraft", "data", "news", "advice", "furniture", "luggage",
    };

    private static readonly Dictionary<string, string> IrregularPlurals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["person"] = "people",
        ["man"] = "men",
        ["woman"] = "women",
        ["child"] = "children",
        ["tooth"] = "teeth",
        ["foot"] = "feet",
        ["mouse"] = "mice",
        ["goose"] = "geese",
        ["ox"] = "oxen",
        ["leaf"] = "leaves",
        ["life"] = "lives",
        ["knife"] = "knives",
        ["wife"] = "wives",
        ["half"] = "halves",
        ["self"] = "selves",
        ["wolf"] = "wolves",
        ["cactus"] = "cacti",
        ["focus"] = "foci",
        ["fungus"] = "fungi",
        ["nucleus"] = "nuclei",
        ["syllabus"] = "syllabi",
        ["analysis"] = "analyses",
        ["diagnosis"] = "diagnoses",
        ["thesis"] = "theses",
        ["crisis"] = "crises",
        ["phenomenon"] = "phenomena",
        ["criterion"] = "criteria",
        ["datum"] = "data",
        ["index"] = "indices",
        ["matrix"] = "matrices",
        ["vertex"] = "vertices",
    };

    private static readonly Dictionary<string, string> IrregularSingulars = BuildReverse();

    /// <summary>Returns the plural form of an English <paramref name="word"/>.</summary>
    /// <param name="word">The singular word.</param>
    /// <returns>The plural form, with the original capitalization preserved.</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    /// <example><code>Inflect.Pluralize("category"); // "categories"</code></example>
    public static string Pluralize(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        if (Uncountable.Contains(word))
        {
            return word;
        }

        if (IrregularPlurals.TryGetValue(word, out var irregular))
        {
            return MatchCase(word, irregular);
        }

        var lower = word.ToLowerInvariant();
        string plural;

        if (EndsWithAny(lower, "s", "ss", "sh", "ch", "x", "z"))
        {
            plural = lower + "es";
        }
        else if (lower.Length > 1 && lower.EndsWith('y') && !IsVowel(lower[^2]))
        {
            plural = lower[..^1] + "ies";
        }
        else if (lower.EndsWith("fe", StringComparison.Ordinal))
        {
            plural = lower[..^2] + "ves";
        }
        else if (lower.EndsWith('f'))
        {
            plural = lower[..^1] + "ves";
        }
        else if (lower.Length > 1 && lower.EndsWith('o') && !IsVowel(lower[^2]))
        {
            plural = lower + "es";
        }
        else
        {
            plural = lower + "s";
        }

        return MatchCase(word, plural);
    }

    /// <summary>Returns the singular form of an English <paramref name="word"/>.</summary>
    /// <param name="word">The plural word.</param>
    /// <returns>The singular form, with the original capitalization preserved.</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    /// <example><code>Inflect.Singularize("categories"); // "category"</code></example>
    public static string Singularize(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        if (Uncountable.Contains(word))
        {
            return word;
        }

        if (IrregularSingulars.TryGetValue(word, out var irregular))
        {
            return MatchCase(word, irregular);
        }

        var lower = word.ToLowerInvariant();
        string singular;

        if (lower.EndsWith("ies", StringComparison.Ordinal) && lower.Length > 3)
        {
            singular = lower[..^3] + "y";
        }
        else if (lower.EndsWith("ves", StringComparison.Ordinal))
        {
            singular = lower[..^3] + "f";
        }
        else if (lower.EndsWith("sses", StringComparison.Ordinal) || lower.EndsWith("xes", StringComparison.Ordinal)
            || lower.EndsWith("zes", StringComparison.Ordinal) || lower.EndsWith("ches", StringComparison.Ordinal)
            || lower.EndsWith("shes", StringComparison.Ordinal))
        {
            singular = lower[..^2];
        }
        else if (lower.EndsWith('s') && !lower.EndsWith("ss", StringComparison.Ordinal))
        {
            singular = lower[..^1];
        }
        else
        {
            singular = lower;
        }

        return MatchCase(word, singular);
    }

    /// <summary>Returns the pluralized or singular form of <paramref name="word"/> based on <paramref name="count"/>.</summary>
    /// <param name="word">The base word.</param>
    /// <param name="count">The quantity. A count of 1 yields the singular; any other value yields the plural.</param>
    /// <returns>The correctly inflected word.</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    /// <example><code>Inflect.Word("case", 2); // "cases"</code></example>
    public static string Word(string word, int count) =>
        count == 1 ? Singularize(word) : Pluralize(word);

    /// <summary>Returns the ordinal string for <paramref name="number"/> (e.g. <c>1st</c>, <c>22nd</c>, <c>113th</c>).</summary>
    /// <param name="number">The number to ordinalize.</param>
    /// <returns>The number followed by its ordinal suffix.</returns>
    public static string Ordinalize(int number)
    {
        var suffix = OrdinalSuffix(number);
        return number.ToString(CultureInfo.InvariantCulture) + suffix;
    }

    /// <summary>Returns the ordinal suffix for <paramref name="number"/> (e.g. <c>st</c>, <c>nd</c>, <c>rd</c>, <c>th</c>).</summary>
    /// <param name="number">The number whose suffix is required.</param>
    /// <returns>The two-letter ordinal suffix.</returns>
    public static string OrdinalSuffix(int number)
    {
        var abs = Math.Abs((long)number);
        var lastTwo = abs % 100;
        if (lastTwo is >= 11 and <= 13)
        {
            return "th";
        }

        return (abs % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th",
        };
    }

    private static Dictionary<string, string> BuildReverse()
    {
        var reverse = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in IrregularPlurals)
        {
            reverse[pair.Value] = pair.Key;
        }

        return reverse;
    }

    private static bool EndsWithAny(string value, params string[] suffixes)
    {
        foreach (var suffix in suffixes)
        {
            if (value.EndsWith(suffix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u';

    private static string MatchCase(string original, string transformed)
    {
        if (original.Length == 0 || transformed.Length == 0)
        {
            return transformed;
        }

        var allUpper = true;
        foreach (var c in original)
        {
            if (char.IsLower(c))
            {
                allUpper = false;
                break;
            }
        }

        if (allUpper && original.Length > 1)
        {
            return transformed.ToUpperInvariant();
        }

        if (char.IsUpper(original[0]))
        {
            return char.ToUpperInvariant(transformed[0]) + transformed[1..];
        }

        return transformed;
    }
}
