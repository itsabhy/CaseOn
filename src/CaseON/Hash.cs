using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CaseON;

/// <summary>
/// Computes hash digests of text and returns them as lower-case hexadecimal. Call as
/// <c>CaseON.Hash.Sha256(text)</c> or, after <c>using CaseON;</c>, simply <c>Hash.Sha256(text)</c>.
/// </summary>
/// <remarks>
/// <para>Inputs are hashed as UTF-8 bytes. All methods throw <see cref="ArgumentNullException"/> for <see langword="null"/> input.</para>
/// <para>
/// <b>Security note:</b> <see cref="Md5(string)"/> and <see cref="Sha1(string)"/> are cryptographically
/// broken and are provided only for non-security use cases such as checksums, cache keys and
/// interoperability. For security-sensitive hashing use <see cref="Sha256(string)"/> or
/// <see cref="Sha512(string)"/>. <see cref="Crc32(string)"/>, <see cref="Fnv1a32(string)"/>,
/// <see cref="Fnv1a64(string)"/> and <see cref="XxHash64(string, ulong)"/> are non-cryptographic.
/// </para>
/// </remarks>
public static class Hash
{
    /// <summary>Computes the MD5 digest of <paramref name="input"/> (non-security use only).</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 32-character hex MD5 digest.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Md5(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
#pragma warning disable CA5351 // MD5 is intentionally offered for non-security checksums.
        return ToHex(MD5.HashData(Encoding.UTF8.GetBytes(input)));
#pragma warning restore CA5351
    }

    /// <summary>Computes the SHA-1 digest of <paramref name="input"/> (non-security use only).</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 40-character hex SHA-1 digest.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Sha1(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
#pragma warning disable CA5350 // SHA-1 is intentionally offered for interoperability.
        return ToHex(SHA1.HashData(Encoding.UTF8.GetBytes(input)));
#pragma warning restore CA5350
    }

    /// <summary>Computes the SHA-256 digest of <paramref name="input"/>.</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 64-character hex SHA-256 digest.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Sha256(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    /// <summary>Computes the SHA-512 digest of <paramref name="input"/>.</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 128-character hex SHA-512 digest.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Sha512(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ToHex(SHA512.HashData(Encoding.UTF8.GetBytes(input)));
    }

    /// <summary>Computes the CRC-32 (IEEE 802.3) checksum of <paramref name="input"/>.</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 8-character hex CRC-32 checksum.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Crc32(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var bytes = Encoding.UTF8.GetBytes(input);
        var crc = 0xFFFFFFFFu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }

        crc ^= 0xFFFFFFFFu;
        return crc.ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Computes the 32-bit FNV-1a hash of <paramref name="input"/>.</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 8-character hex FNV-1a 32-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Fnv1a32(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        const uint offset = 2166136261;
        const uint prime = 16777619;
        var hash = offset;
        foreach (var b in Encoding.UTF8.GetBytes(input))
        {
            hash ^= b;
            hash *= prime;
        }

        return hash.ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Computes the 64-bit FNV-1a hash of <paramref name="input"/>.</summary>
    /// <param name="input">The text to hash.</param>
    /// <returns>The 16-character hex FNV-1a 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Fnv1a64(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        const ulong offset = 14695981039346656037;
        const ulong prime = 1099511628211;
        var hash = offset;
        foreach (var b in Encoding.UTF8.GetBytes(input))
        {
            hash ^= b;
            hash *= prime;
        }

        return hash.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Computes the 64-bit xxHash (XXH64) of <paramref name="input"/>.</summary>
    /// <param name="input">The text to hash.</param>
    /// <param name="seed">An optional seed. Defaults to 0.</param>
    /// <returns>The 16-character hex XXH64 hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string XxHash64(string input, ulong seed = 0)
    {
        ArgumentNullException.ThrowIfNull(input);
        var hash = XxHash64Core(Encoding.UTF8.GetBytes(input), seed);
        return hash.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string ToHex(byte[] bytes) =>
        Convert.ToHexString(bytes).ToLowerInvariant();

    private static ulong XxHash64Core(ReadOnlySpan<byte> data, ulong seed)
    {
        const ulong p1 = 11400714785074694791UL;
        const ulong p2 = 14029467366897019727UL;
        const ulong p3 = 1609587929392839161UL;
        const ulong p4 = 9650029242287828579UL;
        const ulong p5 = 2870177450012600261UL;

        var length = data.Length;
        var index = 0;
        ulong hash;

        unchecked
        {
            if (length >= 32)
            {
                var v1 = seed + p1 + p2;
                var v2 = seed + p2;
                var v3 = seed;
                var v4 = seed - p1;

                do
                {
                    v1 = Round(v1, BinaryPrimitives.ReadUInt64LittleEndian(data[index..]));
                    index += 8;
                    v2 = Round(v2, BinaryPrimitives.ReadUInt64LittleEndian(data[index..]));
                    index += 8;
                    v3 = Round(v3, BinaryPrimitives.ReadUInt64LittleEndian(data[index..]));
                    index += 8;
                    v4 = Round(v4, BinaryPrimitives.ReadUInt64LittleEndian(data[index..]));
                    index += 8;
                }
                while (index <= length - 32);

                hash = BitOperations.RotateLeft(v1, 1) + BitOperations.RotateLeft(v2, 7)
                    + BitOperations.RotateLeft(v3, 12) + BitOperations.RotateLeft(v4, 18);
                hash = MergeRound(hash, v1);
                hash = MergeRound(hash, v2);
                hash = MergeRound(hash, v3);
                hash = MergeRound(hash, v4);
            }
            else
            {
                hash = seed + p5;
            }

            hash += (ulong)length;

            while (index + 8 <= length)
            {
                var k1 = Round(0, BinaryPrimitives.ReadUInt64LittleEndian(data[index..]));
                hash ^= k1;
                hash = (BitOperations.RotateLeft(hash, 27) * p1) + p4;
                index += 8;
            }

            if (index + 4 <= length)
            {
                hash ^= BinaryPrimitives.ReadUInt32LittleEndian(data[index..]) * p1;
                hash = (BitOperations.RotateLeft(hash, 23) * p2) + p3;
                index += 4;
            }

            while (index < length)
            {
                hash ^= data[index] * p5;
                hash = BitOperations.RotateLeft(hash, 11) * p1;
                index++;
            }

            hash ^= hash >> 33;
            hash *= p2;
            hash ^= hash >> 29;
            hash *= p3;
            hash ^= hash >> 32;
        }

        return hash;

        static ulong Round(ulong acc, ulong input)
        {
            unchecked
            {
                acc += input * p2;
                acc = BitOperations.RotateLeft(acc, 31);
                acc *= p1;
                return acc;
            }
        }

        static ulong MergeRound(ulong acc, ulong val)
        {
            unchecked
            {
                val = Round(0, val);
                acc ^= val;
                acc = (acc * p1) + p4;
                return acc;
            }
        }
    }
}
