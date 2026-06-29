# Contributing to CaseON

Thanks for your interest in improving CaseON! This guide explains how to get set up and what we expect from contributions.

## Prerequisites

- [.NET SDK 8.0 or later](https://dotnet.microsoft.com/download) (the repo builds for `net8.0` and `net9.0`).

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test
```

The build treats warnings as errors for the library project and runs the .NET analyzers, so please keep the build clean.

## Project layout

```
src/CaseON/          # the shipping library
tests/CaseON.Tests/  # xUnit test suite
```

## Pull request checklist

- [ ] Add or update tests for your change.
- [ ] Keep public APIs documented with XML comments.
- [ ] Update `CHANGELOG.md` under the "Unreleased" section.
- [ ] For any public API change, note whether it is a breaking change.
- [ ] Ensure `dotnet build -c Release` and `dotnet test` pass.

## Coding conventions

- Follow the rules in `.editorconfig` (file-scoped namespaces, `var` where the type is apparent, braces always).
- Prefer BCL functionality over new dependencies. New runtime dependencies require discussion in an issue first.
- Favor allocation-conscious, culture-invariant implementations.

## Reporting bugs and requesting features

Please use the GitHub issue templates. For security-sensitive reports, follow [SECURITY.md](SECURITY.md) instead of opening a public issue.
