# Testing

## Strategy

Tests are deterministic NUnit unit tests over pure string-producing methods. They assert complete output strings rather than partial fragments.

## Commands

```bash
dotnet test
```

To run only the unit-test project:

```bash
dotnet test NuciWeb.UnitTests/NuciWeb.UnitTests.csproj
```

## Coverage

The test project includes `coverlet.collector`, but is marked `ExcludeFromCodeCoverage=true` in its project file. Current tests cover every `HtmlAttributeNames` property and the principal tested `Select` methods. `ByExactValue` and `ByPartialValue` require additional dedicated tests, as noted in the root architecture document.

## Test boundaries

No browser, HTML parser, network service, database, or external process is required. Tests therefore cannot prove behaviour of a consumer's XPath engine.