namespace CaseON.Internal;

/// <summary>Internal Unicode helpers shared across modules.</summary>
internal static class UnicodeInfo
{
    /// <summary>Determines whether a Unicode code point is an emoji or emoji-related symbol.</summary>
    /// <param name="codePoint">The Unicode scalar value.</param>
    /// <returns><see langword="true"/> if the code point falls in a known emoji or pictographic range.</returns>
    public static bool IsEmoji(int codePoint) =>
        codePoint is (>= 0x1F300 and <= 0x1FAFF)   // symbols & pictographs, supplemental, extended-A
            or (>= 0x1F000 and <= 0x1F2FF)          // mahjong, dominoes, playing cards, enclosed
            or (>= 0x2600 and <= 0x27BF)            // misc symbols & dingbats
            or (>= 0x1F1E6 and <= 0x1F1FF)          // regional indicators
            or 0x2640 or 0x2642 or 0x2695
            or (>= 0xFE00 and <= 0xFE0F)            // variation selectors
            or 0x200D                               // zero-width joiner
            or (>= 0x2300 and <= 0x23FF);           // misc technical (includes some emoji)
}
