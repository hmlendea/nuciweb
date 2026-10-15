# Repository structure

## Root

| Path | Responsibility |
|------|----------------|
| `NuciWeb.slnx` | SDK-style solution containing the library and unit-test projects. |
| `README.md` | Installation, contribution, security, conduct, and licence overview. |
| `ARCHITECTURE.md` | Root architectural description. |
| `PRIVACY.md` | Root privacy policy. |
| `SECURITY.md` | Root vulnerability-reporting policy. |
| `CODE_OF_CONDUCT.md` | Community conduct policy. |
| `CONTRIBUTING.md` | Contributor workflow and development commands. |
| `LICENSE` | GNU General Public License v3.0 or later. |

## Production project

`NuciWeb/` is the only production project.

| File | Responsibility |
|------|----------------|
| `HtmlAttributeNames.cs` | Public static properties for common HTML attribute names. |
| `Select.cs` | Public static XPath selector builders. |
| `NuciWeb.csproj` | Targets `net10.0`; disables implicit usings; enables nullable analysis; is not packable. |

## Test project

`NuciWeb.UnitTests/` contains NUnit tests and references the production project.

| File | Responsibility |
|------|----------------|
| `HtmlAttributeNamesTests.cs` | Verifies every exposed attribute-name value. |
| `SelectTests.cs` | Verifies selector output for normal and empty-string inputs. |
| `NuciWeb.UnitTests.csproj` | Configures NUnit, the test SDK, coverlet, and the project reference. |

## Generated directories

`bin/`, `obj/`, and `TestResults/` are generated build or test output. They are not source ownership boundaries and should not be edited manually.