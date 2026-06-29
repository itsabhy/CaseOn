using CaseON.Internal;

namespace CaseON;

public static partial class Text
{
    /// <summary>
    /// Converts a machine-style identifier into human-readable words.
    /// </summary>
    /// <param name="input">The identifier to humanize.</param>
    /// <returns>A spaced, capitalized phrase with acronyms preserved.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>
    /// Text.Humanize("first_name");      // "First Name"
    /// Text.Humanize("XMLHttpRequest");  // "XML Http Request"
    /// </code></example>
    public static string Humanize(string input) =>
        JoinWords(input, " ", WordCasing.CapitalizePreserve, preserveAcronyms: true);

    /// <summary>
    /// Converts a human-readable phrase into a <c>PascalCase</c> identifier.
    /// </summary>
    /// <param name="input">The phrase to convert.</param>
    /// <returns>A valid identifier; an underscore is prefixed if the result would start with a digit.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    /// <example><code>Text.ToIdentifier("First Name"); // "FirstName"</code></example>
    public static string ToIdentifier(string input)
    {
        var pascal = ToPascalCase(input);
        if (pascal.Length > 0 && char.IsDigit(pascal[0]))
        {
            return "_" + pascal;
        }

        return pascal;
    }

    /// <summary>
    /// Converts a human-readable phrase into a <c>PascalCase</c> identifier. Alias of <see cref="ToIdentifier(string)"/>.
    /// </summary>
    /// <param name="input">The phrase to convert.</param>
    /// <returns>The identifier form of <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is null, empty or white-space.</exception>
    public static string Dehumanize(string input) => ToIdentifier(input);
}
