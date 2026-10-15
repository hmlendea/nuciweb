# Dependencies

## Production dependencies

`NuciWeb/NuciWeb.csproj` has no explicit package references. Production code uses only C# language features and the `net10.0` framework-provided `string` type.

## Test dependencies

`NuciWeb.UnitTests/NuciWeb.UnitTests.csproj` references:

| Package | Role |
|---------|------|
| `NUnit` | Test framework and assertions. |
| `NUnit3TestAdapter` | Test discovery integration. |
| `Microsoft.NET.Test.Sdk` | .NET test execution infrastructure. |
| `coverlet.collector` | Coverage collection support. |
| `Moq` | Test mocking package; current tests do not require a mock boundary. |

## Dependency direction

The test project references the production project. The production project does not reference the test project. No third-party runtime integration exists.