using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CaseON;

/// <summary>
/// Lightweight named-placeholder templating. Call as
/// <c>CaseON.Template.Format("Hi {name}", values)</c> or, after <c>using CaseON;</c>, simply
/// <c>Template.Format(...)</c>.
/// </summary>
/// <remarks>
/// Placeholders use single braces, e.g. <c>{name}</c>. Literal braces are written by doubling them
/// (<c>{{</c> and <c>}}</c>).
/// </remarks>
public static partial class Template
{
    [GeneratedRegex(@"\{\{|\}\}|\{([A-Za-z_][A-Za-z0-9_]*)\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PlaceholderRegex();

    /// <summary>Replaces <c>{key}</c> placeholders in <paramref name="template"/> with values from <paramref name="values"/>.</summary>
    /// <param name="template">The template text.</param>
    /// <param name="values">The placeholder values, keyed by name.</param>
    /// <param name="throwOnMissing">When <see langword="true"/>, an unknown placeholder throws; otherwise it is left untouched.</param>
    /// <returns>The formatted text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="template"/> or <paramref name="values"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="throwOnMissing"/> is <see langword="true"/> and a placeholder has no value.</exception>
    public static string Format(string template, IReadOnlyDictionary<string, object?> values, bool throwOnMissing = false)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);

        return Replace(template, name =>
        {
            if (values.TryGetValue(name, out var value))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return throwOnMissing
                ? throw new KeyNotFoundException($"No value supplied for placeholder '{name}'.")
                : null;
        });
    }

    /// <summary>Replaces <c>{Property}</c> placeholders in <paramref name="template"/> with the public properties of <paramref name="model"/>.</summary>
    /// <param name="template">The template text.</param>
    /// <param name="model">An object whose public property names match placeholders.</param>
    /// <param name="throwOnMissing">When <see langword="true"/>, an unknown placeholder throws; otherwise it is left untouched.</param>
    /// <returns>The interpolated text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="template"/> or <paramref name="model"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="throwOnMissing"/> is <see langword="true"/> and a placeholder has no matching property.</exception>
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Interpolate uses reflection over the model's public properties, which may be trimmed.")]
    public static string Interpolate(string template, object model, bool throwOnMissing = false)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(model);

        var type = model.GetType();
        return Replace(template, name =>
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is not null && property.CanRead)
            {
                return Convert.ToString(property.GetValue(model), CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return throwOnMissing
                ? throw new KeyNotFoundException($"No property matches placeholder '{name}'.")
                : null;
        });
    }

    private static string Replace(string template, Func<string, string?> resolve)
    {
        var builder = new StringBuilder(template.Length);
        var lastIndex = 0;

        foreach (Match match in PlaceholderRegex().Matches(template))
        {
            builder.Append(template, lastIndex, match.Index - lastIndex);

            if (match.Value == "{{")
            {
                builder.Append('{');
            }
            else if (match.Value == "}}")
            {
                builder.Append('}');
            }
            else
            {
                var name = match.Groups[1].Value;
                var resolved = resolve(name);
                builder.Append(resolved ?? match.Value);
            }

            lastIndex = match.Index + match.Length;
        }

        builder.Append(template, lastIndex, template.Length - lastIndex);
        return builder.ToString();
    }
}
