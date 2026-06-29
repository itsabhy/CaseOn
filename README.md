# CaseON

**The all-in-one .NET string toolkit.** Fast, dependency-free, AOT- and trim-friendly, and organized into small, discoverable modules you can call in one line.

```csharp
using CaseON;

Text.ToSnakeCase("HelloWorld");      // "hello_world"
Validate.Email("user@example.com");  // true
Country.GetCallingCode(CountryCode.India); // 91
Slug.Generate("Héllo, World!");      // "hello-world"
Hash.Sha256("abc");                  // "ba7816bf8f01..."
```

Add one `using CaseON;` and every module is available as `Module.Method(...)` — no setup, no DI, no configuration.

## Install

```bash
dotnet add package CaseON
```

Targets `net8.0` and `net9.0`. Zero runtime dependencies.

## Modules at a glance

| Module | What it does | Examples |
| --- | --- | --- |
| `Text` | Case conversion, humanize, truncate | `ToSnakeCase`, `ToPascalCase`, `ToConstantCase`, `ToTrainCase`, `Humanize`, `ToIdentifier`, `Truncate`, `Convert` |
| `Validate` | Format validators (return `bool`) | `Email`, `Url`, `IPv4`, `Guid`, `CreditCard`, `Iban`, `Isbn`, `SemVer`, `Slug`, `PostalCode`, `PhoneNumber` |
| `Country` | ISO 3166-1 data | `GetCallingCode`, `GetInfo`, `GetName`, `TryParse`, `All` |
| `Similarity` | Edit distance & similarity | `Levenshtein`, `Damerau`, `Hamming`, `Jaro`, `JaroWinkler`, `Lcs`, `SorensenDice`, `Jaccard`, `Cosine`, `Compute` |
| `Phonetic` | Sounds-like encoding | `Soundex`, `Metaphone`, `Nysiis`, `Cologne` (+ `*Equals`) |
| `Fuzzy` | Fuzzy matching | `Score`, `IsMatch`, `BestMatch`, `RankMatches`, `Search` |
| `Extract` | Pull fragments from text | `Emails`, `Urls`, `PhoneNumbers`, `Guids`, `IpAddresses`, `Hashtags`, `Mentions`, `Numbers`, `Domains` |
| `Normalize` | Canonicalize text | `Accents`, `Whitespace`, `LineEndings`, `Unicode`, `Punctuation`, `Digits` |
| `Clean` | Strip unwanted characters | `RemoveEmoji`, `RemoveHtml`, `RemovePunctuation`, `RemoveControlCharacters`, `RemoveNumbers`, `RemoveLetters`, `RemoveSpecialCharacters` |
| `Mask` | Redact sensitive values | `Email`, `Phone`, `CreditCard`, `Iban`, `Ssn`, `Password`, `Custom` |
| `Hash` | Digests as hex | `Md5`, `Sha1`, `Sha256`, `Sha512`, `Crc32`, `Fnv1a32`, `Fnv1a64`, `XxHash64` |
| `Encode` / `Decode` | Binary-to-text & escaping | `Base64`, `Base64Url`, `Base32`, `Base58`, `Base85`, `Hex`, `Url`, `Html`, `JavaScript` |
| `Generate` | Secure random strings | `String`, `Alpha`, `Numeric`, `AlphaNumeric`, `Hex`, `Base64`, `Token`, `Guid`, `Password` |
| `Compare` | Comparison helpers | `Equals`, `StartsWith`, `EndsWith`, `Contains`, `NaturalCompare`, `NaturalComparer` |
| `Search` | Search & character tests | `ContainsWord`, `ContainsPhrase`, `IsMatch`, `ContainsAny`, `ContainsAll`, `ContainsOnly`, `IsLetters`, `IsDigits`, `IsSymbols` |
| `Tokenize` | Split text | `Words`, `Sentences`, `Lines`, `Paragraphs`, `Camel`, `Pascal`, `Snake`, `Kebab` |
| `Slug` | URL slugs | `Generate` |
| `Inflect` | English inflection | `Pluralize`, `Singularize`, `Word`, `Ordinalize` |
| `Template` | Named placeholders | `Format`, `Interpolate` |
| `Metrics` | Content metrics | `WordCount`, `SentenceCount`, `UniqueWords`, `ReadingTime`, `CharacterFrequency`, `Entropy` |
| `Analyze` | Aggregate stats | `Text` → `TextStats` |
| `Password` | Strength & policy | `Strength`, `Entropy`, `EstimatedCrackTime`, `Validate`, `HasUppercase`, `HasDigit`, `HasSymbol` |
| `RegexTools` | Safe regex helpers | `Escape`, `Unescape`, `IsMatch`, `Replace`, `Matches`, `NamedGroups` |
| `Diff` | LCS diff | `Lines`, `Words`, `Compare` |
| `Unicode` | Unicode inspection | `Category`, `IsEmoji`, `CombiningMarkCount`, `IsRightToLeft`, `SurrogatePairCount`, `CodePointCount` |
| `Transliterate` | Text → ASCII | `ToAscii` |
| `Language` | Script & language detection | `Script`, `Detect` |
| `Profanity` | Detect & censor | `Contains`, `Find`, `Censor` |

## Quick start

```csharp
using CaseON;

// Casing
Text.ToKebabCase("MyVariableName");          // "my-variable-name"
Text.ToSnakeCase("HTTPServer", preserveAcronyms: true); // "http_server"
Text.Humanize("first_name");                 // "First Name"
Text.Truncate("The quick brown fox", 12);    // "The quick b…"

// Validation
Validate.Iban("GB82 WEST 1234 5698 7654 32"); // true
Validate.PostalCode("M5V 3L9", CountryCode.Canada); // true

// Similarity & fuzzy search
Similarity.JaroWinkler("MARTHA", "MARHTA");  // ~0.961
Fuzzy.BestMatch("aple", new[] { "apple", "grape" })!.Value.Value; // "apple"

// Encoding & hashing
Encode.Base58("hello");                       // "Cn8eVZg"
Hash.XxHash64("abc");                          // non-cryptographic 64-bit hash

// Inflection & slugs
Inflect.Pluralize("category");                // "categories"
Inflect.Ordinalize(21);                       // "21st"
Slug.Generate("Crème Brûlée");                // "creme-brulee"
```

## Design notes

- **Dependency-free & AOT-friendly.** The library has no runtime package dependencies and is marked `IsAotCompatible`/`IsTrimmable`. Linguistic features (transliteration, language detection, profanity) are implemented in-box rather than via reflection/data-file packages, so they work under trimming and Native AOT.
- **Safe by default.** `Validate.*` and `Extract.*` return `false`/empty for `null` input instead of throwing. All regular expressions are source-generated, culture-invariant and guarded with a match timeout to prevent ReDoS.
- **Performance.** Dynamic-programming algorithms use rolling rows and `ArrayPool<T>`; hot paths such as `Similarity.LevenshteinDistance` offer `ReadOnlySpan<char>` overloads.
- **Security note.** `Hash.Md5`/`Hash.Sha1` are provided for checksums and interoperability only — use `Sha256`/`Sha512` for security. Validators perform structural checks, not authoritative verification. `Generate.Token`/`Generate.Password` use a cryptographically secure RNG.

## Versioning

CaseON follows [Semantic Versioning](https://semver.org/). See [CHANGELOG.md](CHANGELOG.md).

## Contributing

Contributions are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md). Security issues: see [SECURITY.md](SECURITY.md).

## License

Licensed under the [MIT License](LICENSE.txt).
