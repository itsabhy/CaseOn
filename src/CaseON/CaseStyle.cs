namespace CaseON;

/// <summary>
/// Identifies a target casing style for <see cref="Text.Convert(string, CaseStyle)"/>.
/// </summary>
public enum CaseStyle
{
    /// <summary><c>lowercase</c>.</summary>
    Lower = 0,

    /// <summary><c>UPPERCASE</c>.</summary>
    Upper,

    /// <summary><c>snake_case</c>.</summary>
    Snake,

    /// <summary><c>kebab-case</c>.</summary>
    Kebab,

    /// <summary><c>PascalCase</c>.</summary>
    Pascal,

    /// <summary><c>camelCase</c>.</summary>
    Camel,

    /// <summary><c>Sentence case</c>.</summary>
    Sentence,

    /// <summary><c>Title Case</c>.</summary>
    Title,

    /// <summary><c>CONSTANT_CASE</c>.</summary>
    Constant,

    /// <summary><c>dot.case</c>.</summary>
    Dot,

    /// <summary><c>slash/case</c>.</summary>
    Slash,

    /// <summary><c>path/case</c>.</summary>
    Path,

    /// <summary><c>Train-Case</c>.</summary>
    Train,

    /// <summary><c>COBOL-CASE</c>.</summary>
    Cobol,

    /// <summary><c>flatcase</c>.</summary>
    Flat,

    /// <summary><c>UPPERFLATCASE</c>.</summary>
    UpperFlat,

    /// <summary><c>aLtErNaTiNg case</c>.</summary>
    Alternating,

    /// <summary><c>iNVERSE case</c> (each character's case inverted).</summary>
    Inverse,

    /// <summary><c>Header-Case</c>.</summary>
    Header,
}
