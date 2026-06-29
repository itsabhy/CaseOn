# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - Unreleased

A ground-up redesign into a modular, discoverable API plus a large feature expansion.
After `using CaseON;`, every capability is reachable as `Module.Method(...)`.

### Added
- **Text** module: full casing catalog (`ToSnakeCase`, `ToKebabCase`, `ToPascalCase`, `ToCamelCase`,
  `ToConstantCase`, `ToDotCase`, `ToSlashCase`/`ToPathCase`, `ToTrainCase`/`ToHeaderCase`, `ToCobolCase`,
  `ToFlatCase`/`ToUpperFlatCase`, `ToSentenceCase`, `ToTitleCase`, `ToAlternatingCase`, `ToInverseCase`),
  a `preserveAcronyms` option, `Humanize`/`Dehumanize`/`ToIdentifier`, smart truncation
  (`Truncate`, `TruncateWords`, `TruncateSentences`, `TruncateMiddle`, `Ellipsis`) and a `Convert(input, CaseStyle)` dispatcher.
- **Validate** module: expanded catalog including `Email`, `Url`, `IPv4`/`IPv6`/`IpAddress`, `MacAddress`,
  `Domain`, `Hostname`, `Port`, `Slug`, `Jwt`, `Guid`, `Identifier`, `SemVer`, `Date`/`Time`/`Iso8601`,
  `Latitude`/`Longitude`/`Coordinate`, `CreditCard`, `Iban`, `Cvv`, `RoutingNumber`, `Isbn`, `Ssn`,
  `BitcoinAddress`, `PhoneNumber`, `Json`, `Xml`, `HtmlElement`, `Base64`, `Hex`, `HexColor`, `FilePath`,
  `FileName`, plus country-aware `PostalCode`/`PhoneNumber`.
- **Country** module + `CountryCode` enum (ISO 3166-1) + `CountryInfo` with a `FrozenDictionary` dataset:
  `GetInfo`, `GetCallingCode`, `GetName`, `TryParse`, `All`.
- New modules: **Similarity** (Levenshtein, Damerau, Hamming, Jaro, Jaro-Winkler, LCS, Sørensen-Dice,
  Jaccard, Cosine + `Compute`/`MatchOptions`), **Phonetic** (Soundex, Metaphone, NYSIIS, Cologne),
  **Fuzzy** (`Score`, `BestMatch`, `RankMatches`, `Search`), **Extract**, **Normalize**, **Clean**,
  **Mask**, **Hash** (incl. in-box CRC32/FNV-1a/XXHash64), **Encode**/**Decode** (Base64/Base64Url/Base32/
  Base58/Base85/Hex/Url/Html/JavaScript), **Generate** (CSPRNG-backed), **Compare** (incl. natural sort),
  **Search**, **Tokenize**, **Slug**, **Inflect**, **Template**, **Metrics**, **Analyze**, **Password**,
  **RegexTools**, **Diff**, **Unicode**, **Transliterate**, **Language**, **Profanity**.
- `ReadOnlySpan<char>` overloads on `Similarity` hot paths.
- Comprehensive xUnit test suite (known vectors, ReDoS regression, `[0,1]` bounds, crypto-token uniqueness, country data).
- CI (GitHub Actions), CodeQL, Dependabot, `.editorconfig`, and shared `Directory.Build.props`.

### Changed
- **BREAKING:** modules renamed — `ConvertON` → `Text`, `ValidateON` → `Validate`, `MatchON` → `Similarity`.
- **BREAKING:** validator methods dropped the `IsValid`/`Is` prefixes (e.g. `IsValidEmail` → `Validate.Email`).
- **BREAKING:** `PostalCodeCountry` replaced by `CountryCode`; postal-code validation moved to `Validate.PostalCode(value, country)`.
- **BREAKING:** `StringExtensions` removed in favor of the facade-style module API.
- Validators accept `string?` and consistently return `false` for null/empty/whitespace.
- All regular expressions are source-generated or compiled-and-cached, culture-invariant, with a 1-second match timeout.
- Library is `IsAotCompatible`/`IsTrimmable`; linguistic features are implemented in-box (no reflection/data-file dependencies).

### Fixed
- **Security:** removed catastrophic-backtracking (ReDoS) pattern in HTML validation.
- **Correctness:** ISBN-10/13 checksums include the check digit (and ISBN-10 `X`).
- Culture-dependent casing replaced with invariant variants throughout.

### Removed
- Legacy `ConvertON`, `ValidateON`, `MatchON`, `StringExtensions`, `PostalCodeCountry` types.
- Unused runtime dependencies and the abandoned `ValidateONService` work-in-progress file.

[2.0.0]: https://github.com/itsabhy/CaseOn/releases/tag/v2.0.0
