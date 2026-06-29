# Security Policy

## Supported versions

| Version | Supported |
| ------- | --------- |
| 2.x     | ✅        |
| < 2.0   | ❌        |

## Reporting a vulnerability

Please **do not** open a public issue for security vulnerabilities.

Instead, report them privately via [GitHub Security Advisories](https://github.com/itsabhy/CaseOn/security/advisories/new). Include:

- A description of the vulnerability and its impact.
- Steps to reproduce (a minimal code sample is ideal).
- Affected version(s).

We aim to acknowledge reports within 72 hours and to provide a remediation timeline after triage.

## Scope notes

CaseON's validators are **structural/heuristic** checks, not authoritative verification. Do not rely on `IsValidEmail`, `IsValidUrl`, `IsValidHtml`, or `IsValidAddress` as security boundaries (for example, to sanitize untrusted input for injection prevention). Use purpose-built encoders/validators for those scenarios.
