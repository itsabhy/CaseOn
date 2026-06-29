using System.Text;

namespace CaseON;

/// <summary>
/// Generates URL-friendly slugs from arbitrary text. Call as <c>CaseON.Slug.Generate("Hello, World!")</c>
/// or, after <c>using CaseON;</c>, simply <c>Slug.Generate(...)</c>.
/// </summary>
public static class Slug
{
    /// <summary>Generates a slug from <paramref name="input"/>.</summary>
    /// <param name="input">The text to slugify.</param>
    /// <param name="separator">The word separator. Defaults to <c>-</c>.</param>
    /// <param name="lowercase">When <see langword="true"/> (default), the slug is lower-cased.</param>
    /// <returns>A slug containing only ASCII letters, digits and separators.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <example><code>Slug.Generate("Héllo, World!"); // "hello-world"</code></example>
    public static string Generate(string input, char separator = '-', bool lowercase = true)
    {
        ArgumentNullException.ThrowIfNull(input);

        var ascii = Normalize.Accents(input);
        var builder = new StringBuilder(ascii.Length);
        var pendingSeparator = false;

        foreach (var c in ascii)
        {
            if (char.IsLetterOrDigit(c) && char.IsAscii(c))
            {
                if (pendingSeparator && builder.Length > 0)
                {
                    builder.Append(separator);
                }

                pendingSeparator = false;
                builder.Append(lowercase ? char.ToLowerInvariant(c) : c);
            }
            else
            {
                pendingSeparator = true;
            }
        }

        return builder.ToString();
    }
}
