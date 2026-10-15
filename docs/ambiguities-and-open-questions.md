# Ambiguities and open questions

## Confirmed gaps

- `ByExactValue` and `ByPartialValue` lack dedicated unit tests.
- The library does not define an escaping policy for XPath literals.
- `NuciWeb.csproj` is not packable despite the root README describing NuGet installation; the current project file is authoritative for the checked-out source.

## Questions for future changes

- Should selector input be escaped or rejected when it cannot be represented as a simple XPath string literal?
- Should dedicated tests cover quotes, whitespace, special characters, and exact/partial value methods?
- Should the project be packable if NuGet distribution is intended?

These are documentation observations, not implemented behaviour.