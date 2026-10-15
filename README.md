[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nuciweb)](https://github.com/hmlendea/nuciweb/releases/latest)
[![Build Status](https://github.com/hmlendea/nuciweb/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nuciweb/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NuciWeb)](https://nuget.org/packages/NuciWeb)
[![License](https://img.shields.io/github/license/hmlendea/nuciweb)](https://github.com/hmlendea/nuciweb/blob/master/LICENSE)

# NuciWeb

NuciWeb is a .NET library for constructing XPath expressions that target HTML elements. It provides static selector builders and HTML attribute-name constants for web automation and HTML-processing applications.

## 📑 Table of Contents

- [Capabilities](#-capabilities)
- [Usage](#-usage)
- [Known Limitations](#-known-limitations)
- [Installation](#-installation)
	- [Package Manager Installation](#package-manager-installation)
- [Privacy and Data](#-privacy-and-data)
- [Development](#-development)
	- [Requirements](#requirements)
	- [Setup](#setup)
	- [Build](#build)
	- [Test](#test)
	- [Continuous Integration](#continuous-integration)
	- [Release](#release)
	- [Dependencies](#dependencies)
- [GitHub Actions](#-github-actions)
- [Project Structure](#-project-structure)
	- [Projects and Packages](#projects-and-packages)
	- [Directories](#directories)
- [Architecture](#-architecture)
- [Documentation](#-documentation)
- [API Reference](#-api-reference)
- [Troubleshooting](#-troubleshooting)
- [FAQ](#-faq)
- [Contributing](#-contributing)
- [Code of Conduct](#-code-of-conduct)
- [Security](#-security)
- [Project Engagement](#-project-engagement)
- [License](#-license)

## ✨ Capabilities

- Construct XPath selectors for raw XPath, classes, IDs, names, exact values, and partial values.
- Centralise common HTML attribute names used by selector templates.
- Produce deterministic strings without requiring a browser, HTML parser, or network service.

## 🚀 Usage

```csharp
string selector = NuciWeb.Select.ById("submit");
```

The returned value is an XPath string that the consuming application can pass to its own browser automation or HTML-processing API. For example, `ByClass("primary-button")` returns:

```text
//*[contains(concat(' ', normalize-space(@class), ' '), ' primary-button ')]
```

## ⚠️ Known Limitations

- NuciWeb constructs XPath strings but does not evaluate them against HTML.
- Input values are interpolated directly and are not XPath-escaped or validated.
- The library targets `net10.0` only.

## 📦 Installation

[![Obtain it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciWeb)
[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/nuciweb/releases)

### Package Manager Installation

```bash
dotnet add package NuciWeb
```

Or, via the `Package Manager Console`:
```powershell
Install-Package NuciWeb
```

## 🛡️ Privacy and Data

NuciWeb does not collect, process, store, or transmit personal data. Consumer-provided strings are used to construct XPath results in memory; any data passed to or retained by a consuming application remains under that application's control.

For the detailed description of how the application handles privacy and personal data, see [PRIVACY.md](./PRIVACY.md).

## 🛠️ Development

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

```bash
git clone https://github.com/hmlendea/nuciweb.git
cd nuciweb
dotnet restore
```

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Continuous Integration

The `.NET` workflow runs on pushes and pull requests targeting `master`. It restores dependencies, builds without restoring again, and runs the test suite on Ubuntu with .NET 10.

### Release

Published GitHub releases trigger package creation in `github-release.yml` and NuGet publication in `nuget-release.yml`. Both workflows derive the package version from the release tag and create a Release configuration package. The GitHub Release workflow uploads the package and appends its SHA256 checksum to the release notes; the NuGet workflow publishes through NuGet's OIDC login flow.

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `NUnit` | `4.2.2` | Test | Unit-test framework. |
| `NUnit3TestAdapter` | `4.6.0` | Test | Test discovery adapter. |
| `Microsoft.NET.Test.Sdk` | `17.12.0` | Test | Test execution infrastructure. |
| `coverlet.collector` | `6.0.2` | Test | Coverage collection support. |
| `Moq` | `4.20.72` | Test | Mocking support for tests. |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [.NET](https://github.com/hmlendea/nuciweb/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Restores, builds, and tests pushes and pull requests targeting `master`. |
| [GitHub Release](https://github.com/hmlendea/nuciweb/blob/master/.github/workflows/github-release.yml) | GitHub Release packaging | Packs the project, uploads the NuGet package to the published release, and records its SHA256 checksum. |
| [NuGet.org Release](https://github.com/hmlendea/nuciweb/blob/master/.github/workflows/nuget-release.yml) | NuGet publication | Packs the project and publishes the package to NuGet.org using OIDC authentication. |

## 🗂️ Project Structure

The solution contains one production library project and one NUnit unit-test project.

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `NuciWeb/NuciWeb.csproj` | .NET library | Provides XPath selector builders and HTML attribute names. |
| `NuciWeb.UnitTests/NuciWeb.UnitTests.csproj` | NUnit test project | Verifies selector output and attribute-name values. |

### Directories

| Directory | Purpose |
|-----------|---------|
| `NuciWeb/` | Production source. |
| `NuciWeb.UnitTests/` | Unit tests. |
| `docs/` | Repository technical documentation. |
| `.github/workflows/` | Continuous integration and release workflows. |

## 🏗️ Architecture

NuciWeb uses a static utility architecture in which pure selector builders return XPath strings without loading or evaluating HTML.

See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 📚 Documentation

See the [documentation index](./docs/INDEX.md) for architecture, API reference, behaviour, flows, testing, and maintenance guidance.

## 🌐 API Reference

See the [API reference](./docs/api-reference/INDEX.md) for the public `Select` and `HtmlAttributeNames` members.

## 🩺 Troubleshooting

See the [troubleshooting guide](./docs/troubleshooting.md) for selector-evaluation, test-discovery, and output-change diagnostics.

## ❓ FAQ

See the [Frequently Asked Questions](./docs/faq.md) for common questions about evaluation, parsing, escaping, state, and testing.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/nuciweb/issues) for problems or suggestions

See the [contributing guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

NuciWeb follows a coordinated disclosure policy for security vulnerabilities.

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nuciweb/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0 or later`.
See [LICENSE](./LICENSE) for additional information.
