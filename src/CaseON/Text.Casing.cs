using System.Globalization;
using System.Text;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// String case-conversion and humanization helpers. Call as <c>CaseON.Text.ToSnakeCase("x")</c>
/// or, after <c>using CaseON;</c>, simply <c>Text.ToSnakeCase("x")</c>.
/// </summary>
/// <remarks>
/// All conversions are culture-invariant and thread-safe. Word boundaries are detected at
/// whitespace, <c>_</c>, <c>-</c>, <c>.</c>, <c>/</c>, <c>\</c> separators and at case transitions.
/// Methods throw <see cref="ArgumentException"/> for <see langword="null"/>, empty or white-space input.
/// </remarks>
public static partial class Text
{
    /// <summary>Converts <paramref name="input"/> to <c>UPPERCASE</c> (invariant).</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The upper-cased string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToUpperCase("hello"); // "HELLO"</code></example>
    public static string ToUpperCase(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input.ToUpperInvariant();
    }

    /// <summary>Converts <paramref name="input"/> to <c>lowercase</c> (invariant).</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The lower-cased string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToLowerCase("HELLO"); // "hello"</code></example>
    public static string ToLowerCase(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input.ToLowerInvariant();
    }

    /// <summary>Converts <paramref name="input"/> to <c>snake_case</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact (<c>HTTPServer</c> -&gt; <c>http_server</c>).</param>
    /// <returns>The <c>snake_case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToSnakeCase("ThisIsAString"); // "this_is_a_string"</code></example>
    public static string ToSnakeCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "_", WordCasing.Lower, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>kebab-case</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>kebab-case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToKebabCase("ThisIsAString"); // "this-is-a-string"</code></example>
    public static string ToKebabCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "-", WordCasing.Lower, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>CONSTANT_CASE</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>CONSTANT_CASE</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToConstantCase("ThisIsAString"); // "THIS_IS_A_STRING"</code></example>
    public static string ToConstantCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "_", WordCasing.Upper, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>dot.case</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>dot.case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToDotCase("ThisIsAString"); // "this.is.a.string"</code></example>
    public static string ToDotCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, ".", WordCasing.Lower, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>slash/case</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>slash/case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToSlashCase("ThisIsAString"); // "this/is/a/string"</code></example>
    public static string ToSlashCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "/", WordCasing.Lower, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>path/case</c> (alias of slash case).</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>path/case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToPathCase("ThisIsAString"); // "this/is/a/string"</code></example>
    public static string ToPathCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "/", WordCasing.Lower, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>Train-Case</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>Train-Case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToTrainCase("this is a string"); // "This-Is-A-String"</code></example>
    public static string ToTrainCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "-", WordCasing.Capitalize, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>Header-Case</c> (alias of Train-Case).</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>Header-Case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToHeaderCase("content type"); // "Content-Type"</code></example>
    public static string ToHeaderCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "-", WordCasing.Capitalize, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>COBOL-CASE</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>COBOL-CASE</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToCobolCase("this is a string"); // "THIS-IS-A-STRING"</code></example>
    public static string ToCobolCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, "-", WordCasing.Upper, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>PascalCase</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact (e.g. <c>XMLHttp</c> -&gt; <c>XMLHttp</c>).</param>
    /// <returns>The <c>PascalCase</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToPascalCase("this is a string"); // "ThisIsAString"</code></example>
    public static string ToPascalCase(string input, bool preserveAcronyms = false) =>
        JoinWords(input, string.Empty, preserveAcronyms ? WordCasing.CapitalizePreserve : WordCasing.Capitalize, preserveAcronyms);

    /// <summary>Converts <paramref name="input"/> to <c>camelCase</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="preserveAcronyms">When <see langword="true"/>, keeps acronyms intact.</param>
    /// <returns>The <c>camelCase</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToCamelCase("this is a string"); // "thisIsAString"</code></example>
    public static string ToCamelCase(string input, bool preserveAcronyms = false)
    {
        var pascal = ToPascalCase(input, preserveAcronyms);
        if (pascal.Length == 0)
        {
            return pascal;
        }

        return string.Create(pascal.Length, pascal, static (span, source) =>
        {
            source.AsSpan().CopyTo(span);
            span[0] = char.ToLowerInvariant(span[0]);
        });
    }

    /// <summary>Converts <paramref name="input"/> to <c>flatcase</c> (lower-cased, no separators).</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The <c>flatcase</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToFlatCase("This Is A String"); // "thisisastring"</code></example>
    public static string ToFlatCase(string input) =>
        JoinWords(input, string.Empty, WordCasing.Lower, preserveAcronyms: false);

    /// <summary>Converts <paramref name="input"/> to <c>UPPERFLATCASE</c> (upper-cased, no separators).</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The <c>UPPERFLATCASE</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToUpperFlatCase("This Is A String"); // "THISISASTRING"</code></example>
    public static string ToUpperFlatCase(string input) =>
        JoinWords(input, string.Empty, WordCasing.Upper, preserveAcronyms: false);

    /// <summary>Converts <paramref name="input"/> to <c>Sentence case</c> (first letter upper, remainder lower).</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The <c>Sentence case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToSentenceCase("this IS a STRING"); // "This is a string"</code></example>
    public static string ToSentenceCase(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var lowered = input.ToLowerInvariant();
        return string.Create(lowered.Length, lowered, static (span, source) =>
        {
            source.AsSpan().CopyTo(span);
            span[0] = char.ToUpperInvariant(span[0]);
        });
    }

    /// <summary>Converts <paramref name="input"/> to <c>Title Case</c>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The <c>Title Case</c> string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToTitleCase("this is a string"); // "This Is A String"</code></example>
    public static string ToTitleCase(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }

    /// <summary>Converts <paramref name="input"/> to <c>aLtErNaTiNg case</c> (alternating per letter, starting lower).</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The alternating-case string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToAlternatingCase("hello world"); // "hElLo wOrLd"</code></example>
    public static string ToAlternatingCase(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var buffer = new char[input.Length];
        var upper = false;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsLetter(c))
            {
                buffer[i] = upper ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
                upper = !upper;
            }
            else
            {
                buffer[i] = c;
            }
        }

        return new string(buffer);
    }

    /// <summary>Inverts the case of every character in <paramref name="input"/>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The case-inverted string (e.g. <c>Hello</c> -&gt; <c>hELLO</c>).</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToInverseCase("Hello World"); // "hELLO wORLD"</code></example>
    public static string ToInverseCase(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var buffer = new char[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            buffer[i] = char.IsUpper(c) ? char.ToLowerInvariant(c)
                : char.IsLower(c) ? char.ToUpperInvariant(c)
                : c;
        }

        return new string(buffer);
    }

    /// <summary>Converts <paramref name="input"/> to the requested <paramref name="style"/>.</summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="style">The target casing style.</param>
    /// <returns>The converted string.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="style"/> is not a defined <see cref="CaseStyle"/>.</exception>
    public static string Convert(string input, CaseStyle style) => style switch
    {
        CaseStyle.Lower => ToLowerCase(input),
        CaseStyle.Upper => ToUpperCase(input),
        CaseStyle.Snake => ToSnakeCase(input),
        CaseStyle.Kebab => ToKebabCase(input),
        CaseStyle.Pascal => ToPascalCase(input),
        CaseStyle.Camel => ToCamelCase(input),
        CaseStyle.Sentence => ToSentenceCase(input),
        CaseStyle.Title => ToTitleCase(input),
        CaseStyle.Constant => ToConstantCase(input),
        CaseStyle.Dot => ToDotCase(input),
        CaseStyle.Slash => ToSlashCase(input),
        CaseStyle.Path => ToPathCase(input),
        CaseStyle.Train => ToTrainCase(input),
        CaseStyle.Cobol => ToCobolCase(input),
        CaseStyle.Flat => ToFlatCase(input),
        CaseStyle.UpperFlat => ToUpperFlatCase(input),
        CaseStyle.Alternating => ToAlternatingCase(input),
        CaseStyle.Inverse => ToInverseCase(input),
        CaseStyle.Header => ToHeaderCase(input),
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Unknown case style."),
    };

    private enum WordCasing
    {
        Lower,
        Upper,
        Capitalize,
        CapitalizePreserve,
    }

    private static string JoinWords(string input, string separator, WordCasing casing, bool preserveAcronyms)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var words = WordTokenizer.Split(input, preserveAcronyms);
        var builder = new StringBuilder(input.Length + (words.Count * separator.Length));

        for (var w = 0; w < words.Count; w++)
        {
            if (w > 0 && separator.Length > 0)
            {
                builder.Append(separator);
            }

            AppendWord(builder, words[w], casing);
        }

        return builder.ToString();
    }

    private static void AppendWord(StringBuilder builder, string word, WordCasing casing)
    {
        switch (casing)
        {
            case WordCasing.Lower:
                foreach (var c in word)
                {
                    builder.Append(char.ToLowerInvariant(c));
                }

                break;

            case WordCasing.Upper:
                foreach (var c in word)
                {
                    builder.Append(char.ToUpperInvariant(c));
                }

                break;

            case WordCasing.CapitalizePreserve when IsAcronym(word):
                builder.Append(word);
                break;

            default:
                builder.Append(char.ToUpperInvariant(word[0]));
                for (var i = 1; i < word.Length; i++)
                {
                    builder.Append(char.ToLowerInvariant(word[i]));
                }

                break;
        }
    }

    private static bool IsAcronym(string word)
    {
        if (word.Length < 2)
        {
            return false;
        }

        foreach (var c in word)
        {
            if (!char.IsUpper(c))
            {
                return false;
            }
        }

        return true;
    }
}
