using System.Collections.Generic;

namespace CaseON;

/// <summary>Qualitative password-strength categories derived from estimated entropy.</summary>
public enum PasswordStrength
{
    /// <summary>Under 28 bits of entropy.</summary>
    VeryWeak = 0,

    /// <summary>28-35 bits of entropy.</summary>
    Weak,

    /// <summary>36-59 bits of entropy.</summary>
    Fair,

    /// <summary>60-127 bits of entropy.</summary>
    Strong,

    /// <summary>128 bits of entropy or more.</summary>
    VeryStrong,
}

/// <summary>Policy options for validating a password.</summary>
/// <param name="MinLength">The minimum acceptable length. Defaults to 8.</param>
/// <param name="RequireUppercase">Whether an upper-case letter is required. Defaults to <see langword="true"/>.</param>
/// <param name="RequireLowercase">Whether a lower-case letter is required. Defaults to <see langword="true"/>.</param>
/// <param name="RequireDigit">Whether a digit is required. Defaults to <see langword="true"/>.</param>
/// <param name="RequireSymbol">Whether a symbol is required. Defaults to <see langword="true"/>.</param>
public readonly record struct PasswordOptions(
    int MinLength = 8,
    bool RequireUppercase = true,
    bool RequireLowercase = true,
    bool RequireDigit = true,
    bool RequireSymbol = true);

/// <summary>The outcome of validating a password against a <see cref="PasswordOptions"/> policy.</summary>
/// <param name="IsValid">Whether the password satisfies every policy requirement.</param>
/// <param name="Errors">The list of unmet requirements (empty when valid).</param>
/// <param name="Strength">The estimated strength category.</param>
/// <param name="Entropy">The estimated entropy in bits.</param>
public sealed record PasswordValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    PasswordStrength Strength,
    double Entropy);

/// <summary>
/// Analyzes and validates passwords. Call as <c>CaseON.Password.Strength("hunter2")</c> or, after
/// <c>using CaseON;</c>, simply <c>Password.Strength(...)</c>.
/// </summary>
/// <remarks>
/// Entropy is estimated from length and the character classes present; it is a heuristic upper bound and
/// does not account for dictionary words or predictable patterns.
/// </remarks>
public static class Password
{
    /// <summary>Determines whether <paramref name="password"/> contains an upper-case letter.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <returns><see langword="true"/> if an upper-case letter is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static bool HasUppercase(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(char.IsUpper);
    }

    /// <summary>Determines whether <paramref name="password"/> contains a lower-case letter.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <returns><see langword="true"/> if a lower-case letter is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static bool HasLowercase(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(char.IsLower);
    }

    /// <summary>Determines whether <paramref name="password"/> contains a digit.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <returns><see langword="true"/> if a digit is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static bool HasDigit(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(char.IsDigit);
    }

    /// <summary>Determines whether <paramref name="password"/> contains a symbol.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <returns><see langword="true"/> if a symbol or punctuation character is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static bool HasSymbol(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Any(static c => char.IsSymbol(c) || char.IsPunctuation(c));
    }

    /// <summary>Estimates the entropy of <paramref name="password"/> in bits.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <returns>The estimated entropy in bits.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static double Entropy(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (password.Length == 0)
        {
            return 0;
        }

        var pool = 0;
        if (HasLowercase(password))
        {
            pool += 26;
        }

        if (HasUppercase(password))
        {
            pool += 26;
        }

        if (HasDigit(password))
        {
            pool += 10;
        }

        if (HasSymbol(password))
        {
            pool += 32;
        }

        if (pool == 0)
        {
            pool = 1;
        }

        return password.Length * Math.Log2(pool);
    }

    /// <summary>Categorizes the strength of <paramref name="password"/>.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <returns>The estimated <see cref="PasswordStrength"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static PasswordStrength Strength(string password)
    {
        var entropy = Entropy(password);
        return entropy switch
        {
            >= 128 => PasswordStrength.VeryStrong,
            >= 60 => PasswordStrength.Strong,
            >= 36 => PasswordStrength.Fair,
            >= 28 => PasswordStrength.Weak,
            _ => PasswordStrength.VeryWeak,
        };
    }

    /// <summary>Estimates the time to brute-force <paramref name="password"/> at a given guess rate.</summary>
    /// <param name="password">The password to inspect.</param>
    /// <param name="guessesPerSecond">The assumed attacker guess rate. Defaults to 10 billion (offline fast hash).</param>
    /// <returns>The estimated average crack time, capped at <see cref="TimeSpan.MaxValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="guessesPerSecond"/> is not positive.</exception>
    public static TimeSpan EstimatedCrackTime(string password, double guessesPerSecond = 1e10)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(guessesPerSecond);

        var entropy = Entropy(password);
        // Average guesses needed is half of the keyspace (2^entropy).
        var seconds = Math.Pow(2, entropy - 1) / guessesPerSecond;
        if (double.IsInfinity(seconds) || seconds > TimeSpan.MaxValue.TotalSeconds)
        {
            return TimeSpan.MaxValue;
        }

        return TimeSpan.FromSeconds(seconds);
    }

    /// <summary>Validates <paramref name="password"/> against the supplied <paramref name="options"/>.</summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="options">The policy options. When omitted, the documented defaults (length 8, all classes required) apply.</param>
    /// <returns>A <see cref="PasswordValidationResult"/> describing validity, unmet rules, strength and entropy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
    public static PasswordValidationResult Validate(string password, PasswordOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(password);

        // 'new PasswordOptions()' applies the record's parameter defaults, unlike 'default'.
        var policy = options ?? new PasswordOptions();
        var minLength = policy.MinLength <= 0 ? 8 : policy.MinLength;

        var errors = new List<string>();
        if (password.Length < minLength)
        {
            errors.Add($"Password must be at least {minLength} characters long.");
        }

        if (policy.RequireUppercase && !HasUppercase(password))
        {
            errors.Add("Password must contain an upper-case letter.");
        }

        if (policy.RequireLowercase && !HasLowercase(password))
        {
            errors.Add("Password must contain a lower-case letter.");
        }

        if (policy.RequireDigit && !HasDigit(password))
        {
            errors.Add("Password must contain a digit.");
        }

        if (policy.RequireSymbol && !HasSymbol(password))
        {
            errors.Add("Password must contain a symbol.");
        }

        return new PasswordValidationResult(errors.Count == 0, errors, Strength(password), Entropy(password));
    }
}
